using System.Collections.ObjectModel;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ToDo.Api.Features.SearchById;
using ToDo.Api.Infrastructure.DataAccess;

namespace ToDo.Api.Features.GetAll;

internal static class Operations
{
    private static Results<ProblemHttpResult, NoContent, Ok<TodoListResponse>> ToResponse(this List<TodoDataModel> todos) =>
        todos.Any()
            ? TypedResults.Ok(
                new TodoListResponse(
                    new ReadOnlyCollection<TodoResponse>(
                        todos.Select(x => new TodoResponse(x.Id, x.Title, x.Description, x.DueDate)).ToList()
                    )
                )
            )
            : TypedResults.NoContent();

    public static async ValueTask<Results<ProblemHttpResult, NoContent, Ok<TodoListResponse>>> ExecuteAsync(
        [FromServices] IQueryHandler<SearchAllQuery, List<TodoDataModel>> queryHandler,
        [FromServices] ILogger<Program> logger,
        CancellationToken token = new()
    )
    {
        try
        {
            var results = await queryHandler.QueryAsync(new SearchAllQuery(), token);
            return (results ?? new List<TodoDataModel>()).ToResponse();
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