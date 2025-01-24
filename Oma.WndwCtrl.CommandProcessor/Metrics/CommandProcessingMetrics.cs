using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

namespace Oma.WndwCtrl.CommandProcessor.Metrics;

public class CommandProcessingMetrics
{
  private const string MeterName = "ACaaD.Processing";
  private const string SchedulingDelayName = "acaad.processing.scheduling.delay";

  private const string SuccessfulComponentExecutionsName = "acaad.processing.component.successful";
  private const string FailedComponentExecutionsName = "acaad.processing.component.failed";
  private readonly Counter<long> _failedExecutionCount;

  private readonly Histogram<double> _schedulingDelay;

  private readonly Counter<long> _successfulExecutionCount;

  [SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Won't fix; Meter instances are not meant to be disposed."
  )]
  public CommandProcessingMetrics(IMeterFactory meterFactory)
  {
    Meter meter = meterFactory.Create(MeterName);

    _schedulingDelay = meter.CreateHistogram(
      SchedulingDelayName,
      "ms",
      "Execution delay to scheduled date",
      advice: new InstrumentAdvice<double>
      {
        HistogramBucketBoundaries =
          [0.05, 0.1, 0.25, 0.5, 1, 2, 3, 5, 10, 20, 30, 50, 80, 130,],
      }
    );

    _successfulExecutionCount = meter.CreateCounter<long>(
      SuccessfulComponentExecutionsName,
      description: "Successful scheduled component execution count"
    );

    _failedExecutionCount = meter.CreateCounter<long>(
      FailedComponentExecutionsName,
      description: "Failed scheduled component execution count"
    );
  }

  public void RecordFinishedCommandExecution(ComponentCommandExecutionFinished commandExecution)
  {
    TagList tags = default;

    IComponent c = commandExecution.Component;
    tags.Add("component.type", c.Type);
    tags.Add("component.name", c.Name);

    if (commandExecution is ComponentCommandExecutionSucceeded)
    {
      _successfulExecutionCount.Add(delta: 1, tags);
    }
    else
    {
      _failedExecutionCount.Add(delta: 1, tags);
    }
  }

  public void RecordSchedulingDelay(ComponentToRunEvent componentToRun)
  {
    if (componentToRun.DelayedBy is not null)
    {
      _schedulingDelay.Record(componentToRun.DelayedBy.Value.TotalMicroseconds / 1000);
    }
  }
}