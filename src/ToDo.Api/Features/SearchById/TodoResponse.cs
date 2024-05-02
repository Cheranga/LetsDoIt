namespace ToDo.Api.Features.SearchById;

public record TodoResponse(string Id, string Title, string Description, DateTimeOffset DueDate);