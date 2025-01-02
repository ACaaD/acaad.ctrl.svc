using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.CoreAsp.Api.Filters;

public record RequestReceivedMessage(string correlationId) : IMessage
{
  public string Topic => "Event";
}

public record RequestProcessedMessage(string correlationId, TimeSpan duration) : IMessage
{
  public string Topic => "Event";
}

public class RequestReceivedActionFilter : IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    Lazy<IMessageBus> messageBusLazy =
      context.HttpContext.RequestServices.GetRequiredService<Lazy<IMessageBus>>();

    IMessageBus messageBus = messageBusLazy.Value;

    await messageBus.SendAsync(new RequestReceivedMessage(context.HttpContext.TraceIdentifier));

    Stopwatch sw = Stopwatch.StartNew();
    await next();

    await messageBus.SendAsync(
      new RequestProcessedMessage(context.HttpContext.TraceIdentifier, sw.Measure())
    );
  }
}