using LanguageExt;
using Oma.WndwCtrl.Abstractions.Metrics;

namespace Oma.WndwCtrl.Abstractions.Model;

public record CommandState
{
  public CommandState(
    IEnumerable<ICommandExecutor> commandExecutors,
    ICommand command,
    IAcaadCoreMetrics metrics
  )
  {
    CommandExecutors = new Seq<ICommandExecutor>(commandExecutors);
    Command = command;
    Metrics = metrics;
  }

  public Seq<ICommandExecutor> CommandExecutors { get; }
  public ICommand Command { get; }
  public IAcaadCoreMetrics Metrics { get; }

  // Someone will definitely scream at me for this.
  public DateTime StartedAt { get; } = DateTime.UtcNow;
}