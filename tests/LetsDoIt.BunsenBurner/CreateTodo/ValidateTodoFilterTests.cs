using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using ToDo.Api.Features.Create;
using ToDo.Api.Features.SearchById;
using static BunsenBurner.Aaa;

namespace LetsDoIt.BunsenBurner.CreateTodo;

public class ValidateTodoFilterTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ValidateTodoFilterTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Invalid task must return bad request status code")]
    public async Task InvalidTask()
    {
        await Arrange(() =>
            {
                var task = new Fixture().Build<AddTodoDto>()
                    .Without(x=>x.Title)
                    .Create();
                var request = new HttpRequestMessage(HttpMethod.Post, "/todos") { Content = JsonContent.Create(task) };
                return request;
            })
            .Act(async request =>
            {
                var httpResponse = await _client.SendAsync(request);
                return httpResponse;
            })
            .Assert(
                (_, response) => { response.StatusCode.Should().Be(HttpStatusCode.BadRequest); }
            )
            .And(async response =>
            {
                var problemResponse = await JsonSerializer.DeserializeAsync<HttpValidationProblemDetails>(
                    await response.Content.ReadAsStreamAsync()
                );
                problemResponse.Should().NotBeNull();
                problemResponse!.Errors.TryGetValue("Title", out var errorMessage).Should().BeTrue();
                errorMessage.Should().NotBeNullOrEmpty();
                errorMessage!.First().Should().Contain("Title cannot be empty");
            });
    }
    
    [Fact(DisplayName = "Valid task must return created status code")]
    public async Task ValidTask()
    {
        await Arrange(() =>
            {
                var task = new Fixture()
                    .Build<AddTodoDto>()
                    .With(x=>x.DueDate, DateTimeOffset.Now.AddDays(1))
                    .Create();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_client.BaseAddress}todos") { Content = JsonContent.Create(task) };
                return request;
            })
            .Act(async request =>
            {
                var httpResponse = await _client.SendAsync(request);
                return httpResponse;
            })
            .Assert(
                (_, response) => { response.StatusCode.Should().Be(HttpStatusCode.Created); }
            )
            .And(async response =>
            {
                response.Headers.TryGetValues("Location", out var location).Should().BeTrue();
                location.Should().NotBeNullOrEmpty();


                var todoResponse = await JsonSerializer.DeserializeAsync<TodoResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                todoResponse.Should().NotBeNull();
                todoResponse.Id.Should().NotBeNullOrEmpty();
            });
    }
}