using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using ToDo.Api.Features.SearchById;
using ToDo.Api.Infrastructure.DataAccess;

namespace ToDo.Api.Features.GetAll;

internal static class Operations
{
    private static readonly string CacheKey = $"{typeof(Operations).FullName}-AllTasks";

    public static async ValueTask<Results<ProblemHttpResult, NoContent, Ok<TodoListResponse>>> ExecuteAsync(
        [FromServices] IDistributedCache cache,
        [FromServices] IQueryHandler<SearchAllQuery, List<TodoDataModel>> queryHandler,
        [FromServices] ILogger<Program> logger,
        CancellationToken token = new()
    )
    {
        try
        {
            var rawData = await cache.GetAsync(CacheKey, token);
            if (rawData != null)
            {
                using var memoryStream = new MemoryStream(rawData);
                var response = (await JsonSerializer.DeserializeAsync<TodoListResponse>(memoryStream, cancellationToken: token))!;
                return response.Tasks.Any() ? TypedResults.Ok(response) : TypedResults.NoContent();
            }

            var results = await queryHandler.QueryAsync(new SearchAllQuery(), token) ?? new List<TodoDataModel>();
            var todoListResponse = new TodoListResponse
            {
                Tasks =
                [
                    ..results.Select(x => new TodoResponse(x.Id, x.Title, x.Description, x.DueDate)).ToList()
                ]
            };

            await cache.SetAsync(
                CacheKey,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(todoListResponse)),
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)},
                token
            );
            return todoListResponse.Tasks.Any() ? TypedResults.Ok(todoListResponse) : TypedResults.NoContent();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "error occurred when getting all the tasks");
        }

        return TypedResults.Problem(
            new ProblemDetails
            {
                Status = (int) HttpStatusCode.InternalServerError,
                Title = "Get all tasks",
                Detail = "error occurred when getting all tasks"
            }
        );
    }
}