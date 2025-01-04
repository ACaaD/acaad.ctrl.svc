using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[PublicAPI]
public record ComponentCommandExecutionSucceeded : ComponentCommandExecutionFinished
{
  public ComponentCommandExecutionSucceeded(
    ComponentExecutingEvent componentExecuting,
    FlowOutcome outcome
  ) : base(
    componentExecuting
  )
  {
    Outcome = outcome;
  }

  public FlowOutcome Outcome { get; }

  public override string Name => nameof(ComponentCommandExecutionSucceeded);
}