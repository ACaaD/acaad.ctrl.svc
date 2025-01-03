using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling.JobFactories;

public class DelegatingJobFactory(ILogger<DelegatingJobFactory> logger, IEnumerable<IJobFactory> jobFactories)
  : IJobFactory
{
  public Option<Job> GetNext(Job current)
  {
    IJobFactory? factory = jobFactories.SingleOrDefault(jf => jf.Handles(current.Trigger));

    if (factory is null)
    {
      logger.LogWarning(
        "Could not locate factory for job type {type}. Job=[{job}]",
        current.GetType(),
        current
      );

      return Option<Job>.None;
    }

    Option<Job> next = factory.GetNext(current).Map(
      j => j with
      {
        Previous = Option<Job>.Some(current with { Previous = Option<Job>.None, }),
      }
    );

    return next;
  }

  public bool Handles(ISchedulableTrigger trigger) => true;

  public Option<Job> CreateJob(DateTime referenceDate, ISchedulableTrigger trigger)
  {
    IJobFactory? factory = jobFactories.SingleOrDefault(jf => jf.Handles(trigger));

    if (factory is not null)
    {
      return factory.CreateJob(referenceDate, trigger);
    }

    logger.LogWarning(
      "Could not locate factory for trigger type {type}. Trigger=[{trigger}]",
      trigger.GetType(),
      trigger
    );

    return Option<Job>.None;
  }
}