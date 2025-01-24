using Microsoft.AspNetCore.SignalR;
using Oma.WndwCtrl.Abstractions.Messaging.Model;

namespace Oma.WndwCtrl.Api.Hubs;

public class EventHubContext(ILogger<EventHubContext> logger, IHubContext<EventHub> hubContext)
{
  internal async Task QueueAsync(Event message, CancellationToken cancelToken)
  {
    try
    {
      await hubContext.Clients.All.SendAsync("receiveEvent", message, cancelToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred sending signalr event.");
    }
  }
}