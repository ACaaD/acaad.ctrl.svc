using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Core.Model.Triggers;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling.JobFactories;

public class RateJobFactory : IJobFactory<RateTrigger>
{
  public Option<Job> CreateJob(DateTime referenceDate, ISchedulableTrigger trigger) =>
    new Job(trigger, referenceDate.AddSeconds(value: 1));

  public Option<Job> GetNext(Job current) => current with
  {
    ScheduledAt = DateTime.UtcNow.AddSeconds(value: 5),
  };
}