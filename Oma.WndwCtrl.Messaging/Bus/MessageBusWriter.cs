using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.Bus;

public class MessageBusWriter(Lazy<IMessageBus?> messageBusLazy) : IMessageBusWriter
{
  public async Task SendAsync(IMessage message, CancellationToken cancelToken = default)
  {
    IMessageBus? messageBus = messageBusLazy.Value;

    if (messageBus is not null)
    {
      await messageBus.SendAsync(message, cancelToken);
    }
  }
}