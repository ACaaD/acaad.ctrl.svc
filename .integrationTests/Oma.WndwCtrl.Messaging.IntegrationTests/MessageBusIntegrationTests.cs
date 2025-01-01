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

  private readonly List<ServiceProvider> _consumerProviders = [];
  private readonly ServiceProvider _serviceProvider;

  private Task? _consumerTask;

  private IMessageBus? _messageBus;

  public MessageBusIntegrationTests()
  {
    _serviceProvider = SetUpMessageBusContainer();
    _cancelToken = TestContext.Current.CancellationToken;
  }

  private IMessageBus MessageBus => _messageBus ??
                                    throw new InvalidOperationException(
                                      $"{nameof(IMessageBus)} is not initialized."
                                    );

  public async ValueTask DisposeAsync()
  {
    (_messageBus as IDisposable)?.Dispose();

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

    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    MessageBus.Complete();
    await WaitForConsumerCompletion();

    consumers.Should().AllSatisfy(
      c =>
        c.Received(requiredNumberOfCalls: 1).OnMessageAsync(
          Arg.Any<DummyMessage>(),
          Arg.Any<CancellationToken>()
        )
    );
  }

  private async Task WaitForConsumerCompletion()
  {
    await (_consumerTask ?? Task.CompletedTask);
  }

  [Fact]
  public async Task ShouldSkipUnregisteredMessageTypes()
  {
    List<IMessageConsumer> consumers = Enumerable
      .Range(start: 0, count: 5)
      .Select(_ => SetUpConsumerContainer<DummyMessage>())
      .ToList();

    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    await MessageBus.SendAsync(new OtherMessage(), _cancelToken);
    MessageBus.Complete();

    await WaitForConsumerCompletion();

    consumers.Should().AllSatisfy(
      c =>
        c.Received(requiredNumberOfCalls: 2).OnMessageAsync(
          Arg.Any<DummyMessage>(),
          Arg.Any<CancellationToken>()
        )
    );
  }

  [Fact]
  public async Task ShouldRouteMessagesByType()
  {
    ServiceCollection services = new();
    IMessageConsumer dummyC = AddConsumerToContainer<DummyMessage>(services);
    IMessageConsumer otherC = AddConsumerToContainer<OtherMessage>(services);

    ServiceProvider provider = services.BuildServiceProvider();
    _consumerTask = provider.StartConsumersAsync(MessageBus, _cancelToken);

    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    await MessageBus.SendAsync(new OtherMessage(), _cancelToken);
    await MessageBus.SendAsync(new OtherMessage(), _cancelToken);
    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    MessageBus.Complete();

    await WaitForConsumerCompletion();

    await dummyC.Received(requiredNumberOfCalls: 3).OnMessageAsync(
      Arg.Any<DummyMessage>(),
      Arg.Any<CancellationToken>()
    );

    await otherC.Received(requiredNumberOfCalls: 2).OnMessageAsync(
      Arg.Any<OtherMessage>(),
      Arg.Any<CancellationToken>()
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

    IMessageConsumer messageConsumer = AddConsumerToContainer<TMessage>(services);

    ServiceProvider result = services.BuildServiceProvider();
    _consumerProviders.Add(result);

    _consumerTask = result.StartConsumersAsync(MessageBus, _cancelToken);

    return messageConsumer;
  }

  private IMessageConsumer AddConsumerToContainer<TMessage>(ServiceCollection services)
    where TMessage : IMessage
  {
    IMessageConsumer<TMessage> messageConsumer = Substitute.For<IMessageConsumer<TMessage>>();

    messageConsumer.IsSubscribedTo(Arg.Any<TMessage>()).Returns(returnThis: true);

    services.AddMessageConsumer<IMessageConsumer<TMessage>, TMessage>(registerConsumer: false);
    services.AddSingleton(messageConsumer);

    return messageConsumer;
  }
}