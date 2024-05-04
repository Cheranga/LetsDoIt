﻿using System.Diagnostics.CodeAnalysis;
using ToDo.Api.Infrastructure.DataAccess;

namespace ToDo.Api.Features.Create;

public record CreateToDoCommand(string Title, string Description, DateTimeOffset DueDate) : ICommand
{
    private string Id => Guid.NewGuid().ToString("N").ToUpper();

    internal record Handler(TodoDbContext Context) : ICommandHandler<CreateToDoCommand>
    {
        public async ValueTask<string> ExecuteAsync(CreateToDoCommand command, CancellationToken token = new())
        {
            var dataModel = new TodoDataModel(command.Id, command.Title, command.Description, command.DueDate);
            await Context.Todos.AddAsync(dataModel, token);
            await Context.SaveChangesAsync(token);

            return dataModel.Id;
        }
    }
}