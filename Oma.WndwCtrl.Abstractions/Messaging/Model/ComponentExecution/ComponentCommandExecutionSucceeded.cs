using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[PublicAPI]
[MustDisposeResource]
public record ComponentCommandExecutionSucceeded : ComponentCommandExecutionFinished
{
  public ComponentCommandExecutionSucceeded(
    ComponentExecutingEvent componentExecuting,
    FlowOutcome outcome
  ) : base(
    componentExecuting
  )
  {
    Outcome = outcome with { };
  }

  public FlowOutcome Outcome { get; }

  public override string Name => nameof(ComponentCommandExecutionSucceeded);
}