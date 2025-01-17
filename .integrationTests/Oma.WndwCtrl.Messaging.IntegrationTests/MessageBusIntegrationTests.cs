using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Extensions;
using Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

namespace Oma.WndwCtrl.Messaging.IntegrationTests;

public sealed class MessageBusIntegrationTests : IAsyncLifetime
{
  private readonly CancellationToken _cancelToken = TestContext.Current.CancellationToken;

  private readonly List<ServiceProvider> _consumerProviders = [];
  private readonly ServiceProvider _serviceProvider = SetUpMessageBusContainer();

  private Task? _consumerTask;

  private IMessageBus? _messageBus;

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

  [Fact]
  [SuppressMessage(
    "ReSharper",
    "UseNameOfInsteadOfTypeOf",
    Justification = "Won't fix; Not enough precision."
  )]
  public async Task ShouldNotSendMessageToRemovedConsumer()
  {
    const string consumerName = "to-remove";
    IMessageConsumer consumer = SetUpConsumerContainer<DummyMessage>(consumerName);

    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    // await Task.Delay(TimeSpan.FromSeconds(seconds: 1), _cancelToken);

    MessageBus.Unregister(consumerName);

    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);
    MessageBus.Complete();

    await WaitForConsumerCompletion();

    await consumer.Received(requiredNumberOfCalls: 1).OnMessageAsync(
      Arg.Any<DummyMessage>(),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task ShouldRecoverFromConsumerErrors()
  {
    IMessageConsumer consumer = SetUpConsumerContainer<DummyMessage>();

    consumer.OnMessageAsync(Arg.Any<DummyMessage>(), Arg.Any<CancellationToken>())
      .Returns(_ => throw new InvalidOperationException("Some temporary error"), _ => Task.CompletedTask);

    for (int i = 0; i < 5; i++)
      await MessageBus.SendAsync(new DummyMessage(), _cancelToken);

    MessageBus.Complete();
    await WaitForConsumerCompletion();

    await consumer.Received(requiredNumberOfCalls: 1).OnExceptionAsync(
      Arg.Any<IMessage>(),
      Arg.Any<Exception>(),
      Arg.Any<CancellationToken>()
    );

    await consumer.Received(requiredNumberOfCalls: 5).OnMessageAsync(
      Arg.Any<DummyMessage>(),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldStopProcessingIfCtIsCancelled()
  {
    using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(_cancelToken);
    SetUpConsumerContainer<DummyMessage>(cancelToken: cts.Token);

    // Not calling message bus complete here -> iteration to be stopped by cancelling ct
    await cts.CancelAsync();

    Func<Task> callback = async () => await WaitForConsumerCompletion();

    await callback.Should().NotThrowAsync();
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldRecoverFromIncorrectStartHandling()
  {
    using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(_cancelToken);
    IMessageConsumer consumer = SetUpConsumerContainer<DummyMessage>(cancelToken: cts.Token);

    consumer.OnStartAsync(Arg.Any<CancellationToken>())
      .Returns(_ => throw new InvalidOperationException("Some temporary error"));

    await cts.CancelAsync();
    Func<Task> callback = async () => await WaitForConsumerCompletion();

    await callback.Should().NotThrowAsync();
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldRecoverFromIncorrectCompletionHandling()
  {
    IMessageConsumer consumer = SetUpConsumerContainer<DummyMessage>();

    consumer.OnCompletedAsync(Arg.Any<CancellationToken>())
      .Returns(_ => throw new InvalidOperationException("Some temporary error"));

    MessageBus.Complete();
    Func<Task> callback = async () => await WaitForConsumerCompletion();

    await callback.Should().NotThrowAsync();
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldRecoverFromIncorrectExceptionHandling()
  {
    IMessageConsumer consumer = SetUpConsumerContainer<DummyMessage>();

    consumer.OnMessageAsync(Arg.Any<DummyMessage>(), Arg.Any<CancellationToken>())
      .Returns(_ => throw new InvalidOperationException("Some temporary error"), _ => Task.CompletedTask);

    consumer.OnExceptionAsync(Arg.Any<IMessage>(), Arg.Any<Exception>(), Arg.Any<CancellationToken>())
      .Returns(_ => throw new InvalidOperationException("Some temporary error"));

    await MessageBus.SendAsync(new DummyMessage(), _cancelToken);

    MessageBus.Complete();
    Func<Task> callback = async () => await WaitForConsumerCompletion();

    await callback.Should().NotThrowAsync();
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldRecoverFromIncorrectCancellationHandling()
  {
    using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(_cancelToken);
    IMessageConsumer consumer = SetUpConsumerContainer<DummyMessage>(cancelToken: cts.Token);

    consumer.OnCancelledAsync(Arg.Any<Exception>(), Arg.Any<CancellationToken>())
      .Returns(_ => throw new InvalidOperationException("Some temporary error"));

    await cts.CancelAsync();
    Func<Task> callback = async () => await WaitForConsumerCompletion();

    await callback.Should().NotThrowAsync();
  }

  private static ServiceProvider SetUpMessageBusContainer()
  {
    ServiceCollection services = new();

    services.AddMessageBus();

    ServiceProvider result = services.BuildServiceProvider();
    return result;
  }

  private IMessageConsumer SetUpConsumerContainer<TMessage>(
    string? consumerName = null,
    CancellationToken? cancelToken = null
  )
    where TMessage : IMessage
  {
    ServiceCollection services = new();

    IMessageConsumer messageConsumer = AddConsumerToContainer<TMessage>(services, consumerName);

    ServiceProvider result = services.BuildServiceProvider();
    _consumerProviders.Add(result);

    _consumerTask = result.StartConsumersAsync(MessageBus, cancelToken ?? _cancelToken);

    return messageConsumer;
  }

  private static IMessageConsumer AddConsumerToContainer<TMessage>(
    ServiceCollection services,
    string? consumerName = null
  )
    where TMessage : IMessage
  {
    consumerName ??= Guid.NewGuid().ToString();

    IMessageConsumer<TMessage> messageConsumer = Substitute.For<IMessageConsumer<TMessage>>();

    messageConsumer.IsSubscribedTo(Arg.Any<TMessage>()).Returns(returnThis: true);

    services.AddMessageConsumer<IMessageConsumer<TMessage>, TMessage>(
      consumerName,
      registerConsumer: false
    );

    services.AddSingleton(messageConsumer);

    return messageConsumer;
  }
}