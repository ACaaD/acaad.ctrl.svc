namespace Oma.WndwCtrl.Scheduling.Interfaces;

public interface ISchedulingContext
{
  Task UpdateSchedulingOffsetAsync(List<TimeSpan> delays, CancellationToken cancelToken);

  DateTime GetNextExecutionReferenceDate();
}