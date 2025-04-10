using Microsoft.Extensions.Logging;
using Oma.AirVentShaker.Api.Model.Events;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.AirVentShaker.Api.Messaging.Consumers;

public class TimeSeriesPersistorMessageConsumer(ILogger<TimeSeriesPersistorMessageConsumer> logger)
  : IMessageConsumer<GForceValueBatchEvent>
{
  public bool IsSubscribedTo(IMessage message) => message is GForceValueBatchEvent;

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(
      exception,
      "An unexpected error occurred processing event {type}.",
      message.GetType().Name
    );

    return Task.CompletedTask;
  }

  public Task OnMessageAsync(GForceValueBatchEvent message, CancellationToken cancelToken = default)
  {
    IEnumerable<string> stepNames = message.DataPoints
      .DistinctBy(dp => dp.TestStep?.ToString() ?? "none")
      .Select(dp => dp.TestStep?.ToString() ?? "none");

    logger.LogDebug(
      "Received {count} events. Test Steps: [{stepNames}]",
      message.DataPoints.Count,
      string.Join(", ", stepNames)
    );

    return Task.CompletedTask;
  }
}