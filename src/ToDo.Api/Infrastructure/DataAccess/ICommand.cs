namespace ToDo.Api.Infrastructure.DataAccess;

public  interface ICommand;

public
    interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    ValueTask<string> ExecuteAsync(TCommand command, CancellationToken token);
}