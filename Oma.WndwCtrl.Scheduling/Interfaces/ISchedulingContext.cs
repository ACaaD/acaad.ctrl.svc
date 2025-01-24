using JetBrains.Annotations;

namespace Oma.WndwCtrl.Scheduling.Interfaces;

[PublicAPI]
public interface ISchedulingContext
{
  Task UpdateSchedulingOffsetAsync(List<TimeSpan> delays, CancellationToken cancelToken);

  DateTime GetNextExecutionReferenceDate();
}