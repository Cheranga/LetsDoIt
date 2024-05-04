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

namespace LetsDoIt.ToDoApi.Tests.GetAllTasks;

public class GetAllTasksFilterTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact(DisplayName = "Given tasks are cached, when get all endpoint is called, then must return tasks from the cache")]
    public async Task GetAllTasksWhenCached()
    {
        // Arrange
        var httpClient = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<TodoDbContext>)
                    );
                    if (dbContextDescriptor != null)
                        services.Remove(dbContextDescriptor);

                    services.AddDbContext<TodoDbContext>(optionsBuilder => { optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString("N")); });

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

        // Act
        var httpResponse1 = await httpClient.GetAsync("/todos");
        var httpResponse2 = await httpClient.GetAsync("/todos");

        // Assert
        httpResponse1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        httpResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent2 = await httpResponse2.Content.ReadAsStringAsync();
        var todoListResponse = JsonSerializer.Deserialize<TodoListResponse>(responseContent2, Constants.SerializerOptions);
        todoListResponse.Should().NotBeNull();
        todoListResponse!.Tasks.Should().NotBeNull().And.HaveCount(3);
    }
}