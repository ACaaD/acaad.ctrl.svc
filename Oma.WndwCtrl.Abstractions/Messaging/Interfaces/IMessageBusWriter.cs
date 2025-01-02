namespace Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

public interface IMessageBusWriter
{
  Task SendAsync(IMessage message, CancellationToken cancelToken = default);
}