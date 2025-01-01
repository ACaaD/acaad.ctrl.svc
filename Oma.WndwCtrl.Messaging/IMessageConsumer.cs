using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging;

public interface IMessageConsumer
{
  bool IsSubscribedTo(IMessage message);

  Task ConsumeAsync(IMessage message, CancellationToken cancelToken = default);
}