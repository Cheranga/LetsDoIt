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
using static BunsenBurner.Bdd;

namespace LetsDoIt.ToDoApi.BunsenBurner.Tests.GetAllTasks;

public partial class GetAllTasksFilterTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact(DisplayName = "Given tasks are cached, when get all endpoint is called, then must return tasks from the cache")]
    public async Task GetAllTasksWhenCached() =>
        await Given(() =>
            {
                return factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(SetupInMemoryDataStore);
                });
            })
            .And(f =>
            {
                return f.WithWebHostBuilder(builder =>
                    {
                        builder.ConfigureTestServices(services =>
                        {
                            SetupMockedCache(services, [], new Fixture().CreateMany<TodoDataModel>().ToList());
                        });
                    })
                    .CreateClient();
            })
            .When(async client =>
            {
                var httpResponse1 = await client.GetAsync("/todos");
                var httpResponse2 = await client.GetAsync("/todos");

                return (httpResponse1, httpResponse2);
            })
            .Then(responses =>
            {
                responses.httpResponse1.StatusCode.Should().Be(HttpStatusCode.NoContent);
                responses.httpResponse2.StatusCode.Should().Be(HttpStatusCode.OK);
            })
            .And(async responses =>
            {
                var todoListResponse = await GetToDoListResponse(responses.httpResponse2);
                todoListResponse.Should().NotBeNull();
                todoListResponse!.Tasks.Should().NotBeNull().And.HaveCount(3);
            });
}
