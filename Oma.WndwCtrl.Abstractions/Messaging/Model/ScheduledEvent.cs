using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

[PublicAPI]
public record ScheduledEvent(Job Job) : Event
{
  public override string Type => "Scheduling";
  public override string? ComponentName => null;
  public override string Name => nameof(ScheduledEvent);
  public string? FriendlyName => Job.Trigger.FriendlyName;
}