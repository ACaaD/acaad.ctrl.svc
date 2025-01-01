using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging;

public interface IMessageConsumer
{
  bool IsSubscribedTo(IMessage message);

  Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default);

  Task OnStartAsync(CancellationToken cancelToken = default);

  Task OnExceptionAsync(
    IMessage message,
    Exception exception,
    CancellationToken cancelToken = default
  );

  Task OnCompletedAsync(CancellationToken cancelToken = default);
}

public interface IMessageConsumer<TMessage> : IMessageConsumer
  where TMessage : IMessage
{
}