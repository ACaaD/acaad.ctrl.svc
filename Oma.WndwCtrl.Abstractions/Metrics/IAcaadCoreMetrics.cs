namespace Oma.WndwCtrl.Abstractions.Metrics;

public interface IAcaadCoreMetrics
{
  /// <summary>
  /// Records a command execution duration in seconds.
  /// </summary>
  /// <param name="command">The executed command</param>
  /// <param name="duration">The execution duration in seconds</param>
  void RecordCommandExecutionDuration(ICommand command, double duration);

  /// <summary>
  /// Records a transformation chain execution duration in seconds.
  /// </summary>
  /// <param name="command">The executed command</param>
  /// <param name="duration">The execution duration in seconds</param>
  void RecordTransformationExecutionDuration(ICommand command, double duration);
}