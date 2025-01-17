using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Metrics;

namespace Oma.WndwCtrl.Core.Metrics;

public class AcaadCoreMetrics : IAcaadCoreMetrics
{
  private const string MeterName = "ACaaD.Core";
  private const string CommandExecutionDurationName = "acaad.core.command.execution.duration";
  private const string TransformationExecutionDurationName = "acaad.core.transformation.execution.duration";

  private readonly Histogram<double> _commandExecutionDuration;
  private readonly Histogram<double> _transformationExecutionDuration;

  [SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Won't fix; Meter instances are not meant to be disposed."
  )]
  public AcaadCoreMetrics(IMeterFactory meterFactory)
  {
    Meter meter = meterFactory.Create(MeterName);

    _commandExecutionDuration = meter.CreateHistogram(
      CommandExecutionDurationName,
      "s",
      "Duration of command executions",
      advice: new InstrumentAdvice<double>
      {
        HistogramBucketBoundaries =
          [0.1, 0.25, 0.5, 1, 1.5, 2, 2.5, 3.0, 3.5, 4, 4.5, 5, 6, 7, 9, 11, 13, 15,],
      }
    );

    _transformationExecutionDuration = meter.CreateHistogram(
      TransformationExecutionDurationName,
      "s",
      "Duration of transformation chain executions",
      advice: new InstrumentAdvice<double>
      {
        HistogramBucketBoundaries =
          [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.175, 0.25, 0.5, 0.75, 1, 2,],
      }
    );
  }

  /// <inheritdoc />
  public void RecordCommandExecutionDuration(ICommand command, double duration)
  {
    TagList tags = GetTagsByCommand(command);

    _commandExecutionDuration.Record(duration, tags);
  }

  /// <inheritdoc />
  public void RecordTransformationExecutionDuration(ICommand command, double duration)
  {
    TagList tags = GetTagsByCommand(command);

    _transformationExecutionDuration.Record(duration, tags);
  }

  private static TagList GetTagsByCommand(ICommand command)
  {
    TagList tags = default;

    tags.Add("command.category", command.Category);

    command.Component.Match(
      c =>
      {
        tags.Add("component.type", c.Type);
        tags.Add("component.name", c.Name);
      },
      () =>
      {
        tags.Add("component.type", "unknown");
        tags.Add("component.name", "unknown");
      }
    );

    return tags;
  }
}