using System.Threading.Channels;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.Bus;

public class NoOpMessageBus : IMessageBus
{
  public Task SendAsync(IMessage message, CancellationToken cancelToken = default) => Task.CompletedTask;

  public void Register(string consumer, Channel<IMessage> messageChannel)
  {
  }

  public void Unregister(string consumer)
  {
  }

  public void Complete()
  {
  }
}