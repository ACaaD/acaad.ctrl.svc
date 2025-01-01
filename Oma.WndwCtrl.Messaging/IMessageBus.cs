using System.Threading.Channels;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging;

public interface IMessageBus
{
  void Register(string consumer, Channel<IMessage> messageChannel);
  void Unregister(string consumer);

  Task SendAsync(IMessage message, CancellationToken cancelToken = default);
}