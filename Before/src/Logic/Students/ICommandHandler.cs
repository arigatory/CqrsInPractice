using CSharpFunctionalExtensions;

namespace Logic.Students
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }
}
