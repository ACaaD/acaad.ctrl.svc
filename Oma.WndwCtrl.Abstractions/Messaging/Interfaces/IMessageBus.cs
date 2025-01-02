using System.Threading.Channels;

namespace Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

public interface IMessageBus
{
  void Register(string consumer, Channel<IMessage> messageChannel);
  void Unregister(string consumer);

  Task SendAsync(IMessage message, CancellationToken cancelToken = default);

  void Complete();
}