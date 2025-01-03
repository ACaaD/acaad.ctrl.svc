using System.Diagnostics;
using System.Runtime.CompilerServices;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling.Jobs;

public sealed class InMemoryJobList(
  ILogger<InMemoryJobList> logger,
  IJobFactory jobFactory,
  ComponentConfigurationAccessor componentConfigurationAccessor
) : IJobList, IDisposable
{
  private readonly PriorityQueue<Job, DateTime> _jobQueue = new();
  private readonly Mutex _mutex = new();

  public void Dispose()
  {
    _mutex.Dispose();
  }

  public string Name => nameof(InMemoryJobList);

  public Task StoreAsync(CancellationToken cancelToken) => Task.CompletedTask;

  public async Task<int> LoadAsync(DateTime referenceDate, CancellationToken cancelToken)
  {
    logger.LogInformation(
      "Loading jobs and first executions. Note: This job list processor does not take previous service runs (i.e. schedules) into account."
    );

    logger.LogInformation(
      "This means that all previous runs (and therefor wait times in rates) are discarded and the schedule starts now ({now}).",
      DateTime.UtcNow
    );

    Option<int> result = await ExecuteWithMutexAsync(PopulateJobQueue, cancelToken);

    return result.Match(cnt => cnt, None: 0);

    Task<int> PopulateJobQueue(PriorityQueue<Job, DateTime> jobQueue)
    {
      List<Job> jobsFromConfig =
        componentConfigurationAccessor.Configuration.Triggers.Select(
          t => jobFactory.CreateJob(referenceDate, t)
        ).ToList();

      foreach (Job job in jobsFromConfig) _jobQueue.Enqueue(job, job.ScheduledAt);

      return Task.FromResult(jobsFromConfig.Count);
    }
  }

  public Task<int> FlagAllToFireAsync(CancellationToken cancelToken) => throw new NotImplementedException();

  public Task<IEnumerable<Job>> GetScheduledJobsAsync() => Task.FromResult<IEnumerable<Job>>(
    _jobQueue.UnorderedItems
      .Select(pair => pair.Element)
      .OrderBy(job => job.ScheduledAt)
  );

  public async IAsyncEnumerable<Job> GetJobsToExecuteAsync(
    DateTime? referenceTime,
    [EnumeratorCancellation] CancellationToken cancelToken = default
  )
  {
    Stopwatch sw = Stopwatch.StartNew();
    referenceTime ??= DateTime.UtcNow;

    bool obtainedMutex = false;

    try
    {
      await _mutex.WaitOneAsync(cancelToken);
      obtainedMutex = true;

      while (!cancelToken.IsCancellationRequested
             && _jobQueue.TryPeek(out Job? j, out DateTime scheduledAt)
             && scheduledAt < referenceTime)
      {
        yield return _jobQueue.Dequeue();

        if (jobFactory.TryGetNext(j, out Job? nextJob))
        {
          _jobQueue.Enqueue(nextJob, nextJob.ScheduledAt);
        }
      }
    }
    finally
    {
      if (obtainedMutex)
      {
        _mutex.ReleaseMutex();
      }

      logger.LogTrace(
        "Processing job list done in {stopwatch}. Reference time: {refDate}",
        sw.Measure(),
        referenceTime
      );
    }
  }

  private async Task<Option<T>> ExecuteWithMutexAsync<T>(
    Func<PriorityQueue<Job, DateTime>, Task<T>> action,
    CancellationToken cancelToken
  )
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    bool obtainedMutex = false;

    try
    {
      await _mutex.WaitOneAsync(cancelToken);
      logger.LogTrace("Waiting for job list mutex finished. This took {elapsed}.", stopwatch.Measure());
      obtainedMutex = true;

      return await action(_jobQueue);
    }
    catch (OperationCanceledException)
    {
      if (obtainedMutex)
      {
        _mutex.ReleaseMutex();
      }

      return Option<T>.None;
    }
  }
}