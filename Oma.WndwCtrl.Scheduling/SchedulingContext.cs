using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Scheduling.Interfaces;

namespace Oma.WndwCtrl.Scheduling;

public class SchedulingContext(ILogger<SchedulingContext> logger) : ISchedulingContext
{
  private const double _dampeningFactor = 0.8d;

  // TODO: Make configurable
  private static readonly long _maxOffsetDelta = TimeSpan.FromMilliseconds(milliseconds: 10).Ticks;
  private static readonly long _minOffsetDelta = TimeSpan.FromMilliseconds(milliseconds: -10).Ticks;
  private TimeSpan _currentOffset = TimeSpan.Zero;

  private long _currentTickOffset;

  public Task UpdateSchedulingOffsetAsync(List<TimeSpan> delays, CancellationToken cancelToken)
  {
    long aggregatedValues = (long)Math.Round(GetDurationsWithoutOutliers(delays).Average(ts => ts.Ticks));

    long calculatedOffsetChange = (long)Math.Round(aggregatedValues * _dampeningFactor);

    long clampedOffsetChange = Math.Clamp(calculatedOffsetChange, _minOffsetDelta, _maxOffsetDelta);

    _currentTickOffset += clampedOffsetChange;
    _currentOffset = TimeSpan.FromTicks(_currentTickOffset);

    logger.LogInformation(
      "Updated scheduling offset to {Offset} based on an average of {Avg} (unfiltered={AvgUnfiltered}) over {Cnt} values.",
      _currentOffset,
      TimeSpan.FromTicks(aggregatedValues),
      TimeSpan.FromMilliseconds(delays.Average(d => d.Milliseconds)),
      delays.Count
    );

    return Task.CompletedTask;
  }

  public DateTime GetNextExecutionReferenceDate() => DateTime.UtcNow + _currentOffset;

  private static IEnumerable<TimeSpan> GetDurationsWithoutOutliers(List<TimeSpan> delays)
  {
    List<TimeSpan> sortedDelays = delays.OrderBy(ts => ts.Ticks).ToList();

    int count = sortedDelays.Count;
    long LQ = sortedDelays[count / 4].Ticks;
    long UQ = sortedDelays[3 * count / 4].Ticks;

    long IQR = UQ - LQ;

    long lowerBound = LQ - (long)(1.5 * IQR);
    long upperBound = UQ + (long)(1.5 * IQR);

    return sortedDelays.Where(ts => ts.Ticks >= lowerBound && ts.Ticks <= upperBound);
  }
}