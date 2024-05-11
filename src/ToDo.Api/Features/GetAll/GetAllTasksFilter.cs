using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ToDo.Api.Features.SearchById;
using ToDo.Api.Infrastructure.DataAccess;

namespace ToDo.Api.Features.GetAll;

internal class GetAllTasksFilter(IDistributedCache cache) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var tasks = new List<TodoDataModel>();
        var rawData = await cache.GetAsync(Constants.CacheKey);
        if (rawData != null && rawData.Any())
        {
            using var memoryStream = new MemoryStream(rawData);
            tasks = await JsonSerializer.DeserializeAsync<List<TodoDataModel>>(memoryStream, Constants.SerializerOptions);
        }

        if (tasks != null && tasks.Any())
            return TypedResults.Ok(
                new TodoListResponse
                {
                    Tasks =
                    [
                        .. tasks
                            .Select(x => new TodoResponse
                            {
                                Id = x.Id,
                                Title = x.Title,
                                Description = x.Description,
                                DueDate = x.DueDate
                            })
                            .ToList()
                    ]
                }
            );

        return await next(context);
    }
}
