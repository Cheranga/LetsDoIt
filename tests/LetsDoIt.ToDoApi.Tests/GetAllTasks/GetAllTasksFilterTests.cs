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

public partial class GetAllTasksFilterTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact(DisplayName = "Given tasks are cached, when get all endpoint is called, then must return tasks from the cache")]
    public async Task GetAllTasksWhenCached()
    {
        // Given
        var httpClient = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    SetupInMemoryDataStore(services);
                    SetupMockedCache(services, [], new Fixture().CreateMany<TodoDataModel>().ToList());
                });
            })
            .CreateClient();

        // When
        var httpResponse1 = await httpClient.GetAsync("/todos");
        var httpResponse2 = await httpClient.GetAsync("/todos");

        // Then
        httpResponse1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        httpResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var todoListResponse = await GetToDoListResponse(httpResponse2);
        todoListResponse.Should().NotBeNull();
        todoListResponse!.Tasks.Should().NotBeNull().And.HaveCount(3);
    }
}
