using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

public class DummyConsumer : IMessageConsumer<DummyMessage>
{
  public virtual bool IsSubscribedTo(IMessage message) => message is DummyMessage;

  public virtual Task OnMessageAsync(DummyMessage message, CancellationToken cancelToken = default) =>
    Task.CompletedTask;

  public virtual Task OnExceptionAsync(
    IMessage message,
    Exception exception,
    CancellationToken cancelToken = default
  ) => Task.CompletedTask;
}