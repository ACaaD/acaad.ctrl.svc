using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling;

[UsedImplicitly]
public sealed class SchedulingDelayEventConsumer(
  ILogger<SchedulingDelayEventConsumer> logger,
  ISchedulingContext schedulingContext
)
  : IMessageConsumer<IMessage>, IDisposable
{
  // TODO: Make configurable
  private const int _chunkSize = 30;

  private readonly SemaphoreSlim _mutex = new(initialCount: 1);

  private ConcurrentBag<TimeSpan> _recentDelays = [];
  private ConcurrentBag<TimeSpan> _recentDelaysOther = [];

  public void Dispose()
  {
    _mutex.Dispose();
  }

  public bool IsSubscribedTo(IMessage message) =>
    message is IHasSchedulingDelay { DelayedBy: not null, };

  public async Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default)
  {
    if (message is IHasSchedulingDelay { DelayedBy: { } delay, })
    {
      logger.LogTrace("Message {msgName} was delayed by {delay}.", message.Name, delay);
      _recentDelays.Add(delay);
    }

    if (_recentDelays.Count > _chunkSize)
    {
      bool acquiredMutex = false;

      try
      {
        await _mutex.WaitAsync(cancelToken);
        acquiredMutex = true;

        if (_recentDelays.Count < _chunkSize)
        {
          return;
        }

        ConcurrentBag<TimeSpan> aggregatedValues = Interlocked.Exchange(
          ref _recentDelays,
          _recentDelaysOther
        );

        _recentDelaysOther = [];

        await ProcessAggregatedDelaysAsync(aggregatedValues.ToList(), cancelToken);
      }
      finally
      {
        if (acquiredMutex)
        {
          _mutex.Release();
        }
      }
    }
  }

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(exception, "An unexpected error occurred processing message {message}.", message);
    return Task.CompletedTask;
  }

  private async Task ProcessAggregatedDelaysAsync(List<TimeSpan> delays, CancellationToken cancelToken)
  {
    await schedulingContext.UpdateSchedulingOffsetAsync(delays, cancelToken);
  }
}