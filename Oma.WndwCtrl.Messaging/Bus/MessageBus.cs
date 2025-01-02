using System.Threading.Channels;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.Bus;

public sealed class MessageBus(MessageBusState state) : IMessageBus, IDisposable
{
  public void Dispose()
  {
    state.Queue.Writer.TryComplete();
  }

  public void Register(string consumer, Channel<IMessage> messageChannel)
  {
    state.Add(consumer, messageChannel);
  }

  public void Unregister(string consumer)
  {
    state.TryRemove(consumer).Do(
      channel => channel.Writer.Complete()
    );
  }

  public Task SendAsync(IMessage message, CancellationToken cancelToken = default)
  {
    state.Queue.Writer.TryWrite(message);
    return Task.CompletedTask;
  }

  public void Complete()
  {
    state.Queue.Writer.Complete();
  }
}