using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Messaging;

namespace Oma.WndwCtrl.Scheduling;

public class TriggerEventConsumer(
  ILogger<TriggerEventConsumer> logger,
  ComponentConfigurationAccessor componentConfigurationAccessor
) : IMessageConsumer<IMessage>
{
  public bool IsSubscribedTo(IMessage message) => message is ScheduledEvent; // TODO: Wrong

  public Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default) =>
    throw new NotImplementedException();

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(exception, "An unexpected error occurred processing message {message}.", message);
    return Task.CompletedTask;
  }
}