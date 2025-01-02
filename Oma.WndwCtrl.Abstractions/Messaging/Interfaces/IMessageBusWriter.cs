using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

public interface IMessageBusWriter
{
  [PublicAPI]
  Task SendAsync(IMessage message, CancellationToken cancelToken = default);
}