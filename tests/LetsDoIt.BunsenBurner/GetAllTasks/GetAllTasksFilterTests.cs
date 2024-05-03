using System.Net;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ToDo.Api.Features.GetAll;
using ToDo.Api.Infrastructure.DataAccess;
using static BunsenBurner.Aaa;

namespace LetsDoIt.BunsenBurner.GetAllTasks;

public class GetAllTasksFilterTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact(DisplayName = "When cached, the tasks must be returned from the cache")]
    public async Task GetAllTasksWhenCached()
    {
        await Arrange(() =>
            {
                return factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var dbContextDescriptor = services.SingleOrDefault(d =>
                            d.ServiceType == typeof(DbContextOptions<TodoDbContext>)
                        );
                        if (dbContextDescriptor != null)
                        {
                            services.Remove(dbContextDescriptor);
                        }

                        services.AddDbContext<TodoDbContext>(optionsBuilder =>
                        {
                            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
                        });
                    });
                });
            })
            .And(f =>
            {
                return f.WithWebHostBuilder(builder =>
                    {
                        builder.ConfigureTestServices(services =>
                        {
                            var tasks = new Fixture().CreateMany<TodoDataModel>().ToList();
                            var mockedCache = new Mock<IDistributedCache>();
                            mockedCache
                                .SetupSequence(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync([])
                                .ReturnsAsync(JsonSerializer.SerializeToUtf8Bytes(tasks, Constants.SerializerOptions));

                            services.AddSingleton(mockedCache.Object);
                        });
                    })
                    .CreateClient();
            })
            .Act(async client =>
            {
                var httpResponse1 = await client.GetAsync("/todos");
                var httpResponse2 = await client.GetAsync("/todos");

                return (httpResponse1, httpResponse2);
            })
            .Assert(responses =>
            {
                responses.httpResponse1.StatusCode.Should().Be(HttpStatusCode.NoContent);
                responses.httpResponse2.StatusCode.Should().Be(HttpStatusCode.OK);
            })
            .And(async responses =>
            {
                var responseContent2 = await responses.httpResponse2.Content.ReadAsStringAsync();
                var todoListResponse = JsonSerializer.Deserialize<TodoListResponse>(
                    responseContent2,
                    Constants.SerializerOptions
                );
                todoListResponse.Should().NotBeNull();
                todoListResponse!.Tasks.Should().NotBeNull().And.HaveCount(3);
            });
    }
}
