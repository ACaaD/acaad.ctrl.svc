using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging;

[PublicAPI]
public interface IMessageConsumer
{
  bool IsSubscribedTo(IMessage message);

  Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default);

  [ExcludeFromCodeCoverage]
  Task OnStartAsync(CancellationToken cancelToken = default) => Task.CompletedTask;

  Task OnExceptionAsync(
    IMessage message,
    Exception exception,
    CancellationToken cancelToken = default
  );

  [ExcludeFromCodeCoverage]
  Task OnCompletedAsync(CancellationToken cancelToken = default) => Task.CompletedTask;

  [ExcludeFromCodeCoverage]
  Task OnCancelledAsync(Exception? ex, CancellationToken cancelToken = default) => Task.CompletedTask;
}

public interface IMessageConsumer<in TMessage> : IMessageConsumer
  where TMessage : IMessage
{
  [ExcludeFromCodeCoverage]
  async Task IMessageConsumer.OnMessageAsync(IMessage message, CancellationToken cancelToken)
  {
    if (message is not TMessage msg)
    {
      await OnExceptionAsync(
        message,
        new InvalidOperationException(
          $"Expected message of type {typeof(TMessage).FullName}, but received {message.GetType().FullName}"
        ),
        cancelToken
      );

      return;
    }

    await OnMessageAsync(msg, cancelToken);
  }

  Task OnMessageAsync(TMessage message, CancellationToken cancelToken = default);
}