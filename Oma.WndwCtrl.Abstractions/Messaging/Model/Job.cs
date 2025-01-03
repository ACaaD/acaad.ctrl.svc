using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

public record Job(ISchedulableTrigger Trigger, DateTime ScheduledAt)
{
  public Option<Job> Previous { get; private set; } = Option<Job>.None;

  public void SetPrevious(Job previous)
  {
    if (previous != Option<Job>.None)
    {
      throw new InvalidOperationException("Previous job is already populated. This operation is invalid.");
    }

    Previous = previous;
  }

  public static Job Initialize(ISchedulableTrigger trigger, DateTime referenceDate)
  {
    DateTime scheduledAt = trigger switch
    {
      var _ => DateTime.MinValue,
    };

    return new Job(trigger, scheduledAt);
  }
}