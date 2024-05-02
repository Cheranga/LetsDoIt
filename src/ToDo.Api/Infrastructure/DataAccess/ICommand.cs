namespace ToDo.Api.Infrastructure.DataAccess;

internal interface ICommand;

internal interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    ValueTask<string> ExecuteAsync(TCommand command, CancellationToken token);
}