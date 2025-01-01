using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Messaging.Consumers;

internal class FanOutMessageConsumer(ILogger<FanOutMessageConsumer> logger, MessageBusState messageBusState)
  : IMessageConsumer<IMessage>
{
  public bool IsSubscribedTo(IMessage message) => true;

  public async Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default)
  {
    await Task.WhenAll(
      messageBusState.ActiveChannels.Select(
        channel => channel.Writer.WriteAsync(message, cancelToken).AsTask()
      )
    );
  }

  [ExcludeFromCodeCoverage]
  public Task OnExceptionAsync(
    IMessage message,
    Exception exception,
    CancellationToken cancelToken = default
  )
  {
    logger.LogError(exception, "An unexpected error occured processing a message.");
    return Task.CompletedTask;
  }

  public Task OnCompletedAsync(CancellationToken cancelToken = default)
  {
    foreach (Channel<IMessage> channel in messageBusState.ActiveChannels) channel.Writer.Complete();
    return Task.CompletedTask;
  }
}