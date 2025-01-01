using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Messaging.Consumers;

internal class FanOutMessageConsumer(MessageBusState messageBusState) : IMessageConsumer<IMessage>
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

  public Task OnStartAsync(CancellationToken cancelToken = default) => Task.CompletedTask;

  // TODO
  public Task OnExceptionAsync(
    IMessage message,
    Exception exception,
    CancellationToken cancelToken = default
  ) => throw new NotImplementedException();

  public Task OnCompletedAsync(CancellationToken cancelToken = default) =>
    // TODO: Complete consumer queues
    Task.CompletedTask;
}