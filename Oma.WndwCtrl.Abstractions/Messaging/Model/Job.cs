using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

public record Job(ISchedulableTrigger Trigger, DateTime ScheduledAt)
{
  public Option<Job> Previous { get; init; } = Option<Job>.None;

  public static Job Initialize(ISchedulableTrigger trigger, DateTime referenceDate)
  {
    DateTime scheduledAt = trigger switch
    {
      var _ => DateTime.MinValue,
    };

    return new Job(trigger, scheduledAt);
  }
}