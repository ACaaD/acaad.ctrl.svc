using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling.Model;

public class SchedulingState(IJobList jobList)
{
}

public class JobList
{
  // private void PopulateJobList(ComponentConfigurationAccessor configurationAccessor)
}

public record Job(ISchedulableTrigger Trigger, DateTime ScheduledAt)
{
  public static Job Initialize(ISchedulableTrigger trigger, DateTime referenceDate)
  {
    DateTime scheduledAt = trigger switch
    {
      var _ => DateTime.MinValue,
    };

    return new Job(trigger, scheduledAt);
  }

  public bool TryGetNext([MaybeNullWhen(returnValue: false)] out Job job)
  {
    job = null;

    return Trigger switch
    {
      var _ => false,
    };
  }
}