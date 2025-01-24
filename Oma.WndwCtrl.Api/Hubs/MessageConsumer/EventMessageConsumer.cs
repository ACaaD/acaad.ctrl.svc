using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

namespace Oma.WndwCtrl.Api.Hubs.MessageConsumer;

[UsedImplicitly]
public class EventMessageConsumer(ILogger<EventMessageConsumer> logger, EventHubContext hubContext)
  : IMessageConsumer<Event>
{
  public bool IsSubscribedTo(IMessage message) =>
    message is ComponentCommandOutcomeEvent or ComponentCommandExecutionSucceeded;

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(
      exception,
      "An unexpected error occurred processing event {type}.",
      message.GetType().Name
    );

    return Task.CompletedTask;
  }

  public async Task OnMessageAsync(Event message, CancellationToken cancelToken = default)
  {
    await hubContext.QueueAsync(message, cancelToken);
  }
}