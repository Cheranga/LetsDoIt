using Microsoft.EntityFrameworkCore;
using ToDo.Api.Infrastructure.DataAccess;

namespace ToDo.Api.Features.GetAll;

public record SearchAllQuery : IQuery
{
    internal record Handler(TodoDbContext Context) : IQueryHandler<SearchAllQuery, List<TodoDataModel>>
    {
        public async ValueTask<List<TodoDataModel>?> QueryAsync(SearchAllQuery query, CancellationToken token)
        {
            var todos = await Context.Todos.AsNoTracking().ToListAsync(token);
            return todos;
        }
    }
}