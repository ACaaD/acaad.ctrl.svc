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
    logger.LogInformation("Received {count} events.", message.DataPoints.Count);

    return Task.CompletedTask;
  }
}