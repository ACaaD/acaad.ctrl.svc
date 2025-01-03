using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.CoreAsp.Api.Filters;

[PublicAPI]
public record RequestReceivedMessage(string correlationId, string path, string method) : IMessage
{
  public string Topic => "Event";
  public string Type => "API";
  public virtual string Name => "RequestReceived";

  public string? ComponentName => null;
}

[PublicAPI]
public record RequestProcessedMessage : RequestReceivedMessage
{
  public RequestProcessedMessage(RequestReceivedMessage receivedMessage, TimeSpan duration) : base(
    receivedMessage
  )
  {
    Duration = duration;
  }

  public override string Name => "RequestProcessed";

  public TimeSpan Duration { get; }
}

[UsedImplicitly]
public class RequestReceivedActionFilter : IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    IMessageBusWriter messageBus =
      context.HttpContext.RequestServices.GetRequiredService<IMessageBusWriter>();

    RequestReceivedMessage requestReceivedMessage = new(
      context.HttpContext.TraceIdentifier,
      context.HttpContext.Request.Path,
      context.HttpContext.Request.Method
    );

    await messageBus.SendAsync(requestReceivedMessage);

    Stopwatch sw = Stopwatch.StartNew();
    await next();

    await messageBus.SendAsync(
      new RequestProcessedMessage(requestReceivedMessage, sw.Measure())
    );
  }
}