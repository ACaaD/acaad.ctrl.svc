using Microsoft.AspNetCore.SignalR;

namespace Oma.WndwCtrl.Api.Hubs;

public class EventHub(ILogger<EventHub> logger, EventHubContext context) : Hub
{
}