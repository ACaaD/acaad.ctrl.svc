using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Model;

public record CommandState
{
  public CommandState(IEnumerable<ICommandExecutor> commandExecutors, ICommand command)
  {
    CommandExecutors = new Seq<ICommandExecutor>(commandExecutors);
    Command = command;
  }

  public Seq<ICommandExecutor> CommandExecutors { get; }
  public ICommand Command { get; }
}