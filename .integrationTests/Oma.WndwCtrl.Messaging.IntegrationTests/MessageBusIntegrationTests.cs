using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Extensions;
using Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

namespace Oma.WndwCtrl.Messaging.IntegrationTests;

public sealed class MessageBusIntegrationTests : IAsyncLifetime
{
  private readonly CancellationToken _cancelToken;
  private readonly List<IDisposable> _consumerDisposables = [];

  private readonly List<ServiceProvider> _consumerProviders = [];
  private readonly ServiceProvider _serviceProvider;

  private IMessageBus _messageBus;

  public MessageBusIntegrationTests()
  {
    _serviceProvider = SetUpMessageBusContainer();
    _cancelToken = TestContext.Current.CancellationToken;
  }

  public async ValueTask DisposeAsync()
  {
    (_messageBus as IDisposable)?.Dispose();
    _consumerDisposables.ForEach(cd => cd.Dispose());

    await _serviceProvider.DisposeAsync();
    await Task.WhenAll(_consumerProviders.Select(x => x.DisposeAsync().AsTask()));
  }

  public ValueTask InitializeAsync()
  {
    _messageBus = _serviceProvider.StartMessageBus(_cancelToken);
    return ValueTask.CompletedTask;
  }

  [Fact]
  public async Task ShouldFanOutMessages()
  {
    List<IMessageConsumer> consumers = Enumerable
      .Range(start: 0, count: 5)
      .Select(_ => SetUpConsumerContainer<DummyMessage>())
      .ToList();

    await _messageBus.SendAsync(new DummyMessage(), _cancelToken);

    consumers.Should().AllSatisfy(
      c =>
        c.Received(requiredNumberOfCalls: 1).ConsumeAsync(
          Arg.Any<IMessage>(),
          Arg.Any<CancellationToken>()
        )
    );
  }

  [Fact]
  public async Task ShouldSkipUnregisteredMessageTypes()
  {
    List<IMessageConsumer> consumers = Enumerable
      .Range(start: 0, count: 5)
      .Select(_ => SetUpConsumerContainer<DummyMessage>())
      .ToList();

    await _messageBus.SendAsync(new DummyMessage(), _cancelToken);
    await _messageBus.SendAsync(new DummyMessage(), _cancelToken);
    await _messageBus.SendAsync(new OtherMessage(), _cancelToken);

    consumers.Should().AllSatisfy(
      c =>
        c.Received(requiredNumberOfCalls: 2).ConsumeAsync(
          Arg.Any<IMessage>(),
          Arg.Any<CancellationToken>()
        )
    );
  }

  private ServiceProvider SetUpMessageBusContainer()
  {
    ServiceCollection services = new();

    services.AddMessageBus();

    ServiceProvider result = services.BuildServiceProvider();
    return result;
  }

  private IMessageConsumer SetUpConsumerContainer<TMessage>()
    where TMessage : IMessage
  {
    ServiceCollection services = new();

    IMessageConsumer messageConsumer = Substitute.For<IMessageConsumer>();

    messageConsumer.IsSubscribedTo(Arg.Any<IMessage>())
      .Returns(msg => typeof(TMessage).IsAssignableFrom(msg[index: 0].GetType()));

    services.AddMessageConsumer<TMessage, IMessageConsumer>();
    services.AddSingleton(messageConsumer);

    ServiceProvider result = services.BuildServiceProvider();
    _consumerProviders.Add(result);

    IDisposable disposable = result.StartConsumers(_messageBus);
    _consumerDisposables.Add(disposable);

    return messageConsumer;
  }
}