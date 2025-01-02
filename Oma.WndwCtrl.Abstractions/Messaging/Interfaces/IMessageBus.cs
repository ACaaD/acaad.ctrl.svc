using System.Threading.Channels;

namespace Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

public interface IMessageBus : IMessageBusWriter
{
  void Register(string consumer, Channel<IMessage> messageChannel);
  void Unregister(string consumer);

  void Complete();
}