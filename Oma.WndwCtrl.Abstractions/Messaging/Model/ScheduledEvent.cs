namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

public record ScheduledEvent(Job Job) : Event
{
  public override string Type => "Scheduling";
  public override string? ComponentName => null;
  public override string Name => nameof(ScheduledEvent);
}