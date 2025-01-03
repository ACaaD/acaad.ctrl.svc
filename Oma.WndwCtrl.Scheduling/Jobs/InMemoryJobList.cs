using System.Runtime.CompilerServices;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;
using Oma.WndwCtrl.Scheduling.Model;

namespace Oma.WndwCtrl.Scheduling.Jobs;

public sealed class InMemoryJobList(
  ILogger<InMemoryJobList> logger,
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

  public Task<int> LoadAsync(CancellationToken cancelToken) => Task.FromResult(result: 0);

  public Task<int> FlagAllToFireAsync(CancellationToken cancelToken)
  {
  }

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

        if (j.TryGetNext(out Job? nextJob))
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
    }
  }
}