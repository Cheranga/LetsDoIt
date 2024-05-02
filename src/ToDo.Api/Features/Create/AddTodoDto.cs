namespace ToDo.Api.Features.Create;

public record AddTodoDto(string Title, string Description, DateTimeOffset DueDate);