using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SignalR;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;

namespace Oma.WndwCtrl.Api.Hubs;

public class EventHub(ILogger<EventHub> logger) : Hub, IMessageConsumer<Event>
{
  public bool IsSubscribedTo(IMessage message) => message is Event;

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(
      exception,
      "An unexpected error occurred processing event {type}.",
      message.GetType().Name
    );

    return Task.CompletedTask;
  }

  [SuppressMessage(
    "ReSharper",
    "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
    Justification = "TBD; SignalR is in poc state."
  )]
  public async Task OnMessageAsync(Event message, CancellationToken cancelToken = default)
  {
    await (Clients?.All?.SendAsync("ReceiveEvent", message, cancelToken) ?? Task.CompletedTask);
  }
}