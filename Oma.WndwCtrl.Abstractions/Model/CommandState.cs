using LanguageExt;
using Microsoft.Extensions.Logging;

namespace Oma.WndwCtrl.Abstractions.Model;

public record CommandState
{
  public Seq<ICommandExecutor> CommandExecutors;
  public ILogger Logger;

  public CommandState(ILogger logger, IEnumerable<ICommandExecutor> commandExecutors, ICommand command)
  {
    Logger = logger;
    CommandExecutors = new Seq<ICommandExecutor>(commandExecutors);
    Command = command;
  }

  public ICommand Command { get; }

  public Option<DateTime> StartDate { get; init; }

  public TimeSpan? ExecutionDuration { get; set; }
  public int? ExecutedRetries { get; set; }
}