namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

public record ComponentExecutingEvent : ComponentEvent
{
  public ComponentExecutingEvent(
    ComponentToRunEvent componentToRun,
    int commandsToExecute,
    DateTime startedAt
  ) : base(
    componentToRun
  )
  {
    CommandsToExecute = commandsToExecute;
    StartedAt = startedAt;
  }

  public int CommandsToExecute { get; }
  public DateTime StartedAt { get; }

  public override string Type => "Scheduling";
  public override string Name => nameof(ComponentExecutingEvent);
}