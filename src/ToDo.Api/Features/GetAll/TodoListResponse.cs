using System.Collections.ObjectModel;
using ToDo.Api.Features.SearchById;

namespace ToDo.Api.Features.GetAll;

public record TodoListResponse(ReadOnlyCollection<TodoResponse> Tasks);