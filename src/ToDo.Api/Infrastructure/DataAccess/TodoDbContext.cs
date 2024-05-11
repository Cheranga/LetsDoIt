using Microsoft.EntityFrameworkCore;

namespace ToDo.Api.Infrastructure.DataAccess;

// This must be a struct as EF works only with reference types
public record TodoDataModel(string Id, string Title, string Description, DateTimeOffset DueDate);

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<TodoDataModel> Todos => Set<TodoDataModel>();
}
