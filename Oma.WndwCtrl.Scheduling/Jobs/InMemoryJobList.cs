using System.Diagnostics;
using System.Runtime.CompilerServices;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling.Jobs;

public sealed class InMemoryJobList(
  ILogger<InMemoryJobList> logger,
  [FromKeyedServices(ServiceKeys.RootJobFactory)]
  IJobFactory jobFactory
) : IJobList, IDisposable
{
  private readonly PriorityQueue<Job, DateTime> _jobQueue = new();
  private readonly SemaphoreSlim _mutex = new(initialCount: 1, maxCount: 1);

  public void Dispose()
  {
    _mutex.Dispose();
  }

  public string Name => nameof(InMemoryJobList);

  public Task StoreAsync(CancellationToken cancelToken) => Task.CompletedTask;

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
      await _mutex.WaitAsync(cancelToken);
      obtainedMutex = true;

      while (!cancelToken.IsCancellationRequested
             && _jobQueue.TryPeek(out Job? j, out DateTime scheduledAt)
             && scheduledAt < referenceTime)
      {
        yield return _jobQueue.Dequeue();

        jobFactory.GetNext(j).Do(next => _jobQueue.Enqueue(next, next.ScheduledAt));
      }
    }
    finally
    {
      if (obtainedMutex)
      {
        _mutex.Release();
      }

      logger.LogTrace(
        "Processing job list done in {stopwatch}. Reference time: {refDate}",
        sw.Measure(),
        referenceTime
      );
    }
  }

  public async Task<int> MergeJobsAsync(IEnumerable<Job> jobsFromConfig, CancellationToken cancelToken)
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
      List<Job> jobList = jobsFromConfig.ToList();
      foreach (Job job in jobList) _jobQueue.Enqueue(job, job.ScheduledAt);

      return Task.FromResult(jobList.Count);
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
      await _mutex.WaitAsync(cancelToken);
      logger.LogTrace("Waiting for job list mutex finished. This took {elapsed}.", stopwatch.Measure());
      obtainedMutex = true;

      return await action(_jobQueue);
    }
    catch (OperationCanceledException)
    {
      return Option<T>.None;
    }
    finally
    {
      if (obtainedMutex)
      {
        _mutex.Release();
      }
    }
  }
}