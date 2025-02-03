using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Model;

namespace Oma.WndwCtrl.Messaging;

public interface IChannelWorker
{
  Task ProcessUntilCompletedAsync(CancellationToken cancelToken = default);
}

internal abstract class ChannelWorker(ILogger logger, IMessageConsumer consumer)
  : IChannelWorker, IAsyncDisposable
{
  private Channel<IMessage>? _channel;
  private CancellationTokenSource? _loopCts;
  private CancellationTokenSource? _onExceptionCts;
  private ChannelSettings? _settings;

  protected Channel<IMessage> Channel =>
    _channel ?? throw new InvalidOperationException("Channel is not initialized.");

  public ValueTask DisposeAsync()
  {
    _onExceptionCts?.Dispose();
    _loopCts?.Dispose();

    return ValueTask.CompletedTask;
  }

  public async Task ProcessUntilCompletedAsync(CancellationToken cancelToken = default)
  {
    _onExceptionCts = new CancellationTokenSource();
    _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _onExceptionCts.Token);

    try
    {
      await OnStartAsync(cancelToken);

      int concurrency = _settings?.Concurrency ?? 16;
      logger.LogInformation("Starting channel worker with {concurrency} tasks.", concurrency);

      await Task.WhenAll(
        Enumerable.Range(start: 0, concurrency)
          .Select(_ => ProcessMessagesAsync(_loopCts.Token))
      );
    }
    catch (OperationCanceledException ex)
    {
      await OnCancelledAsync(ex, CancellationToken.None);
      return;
    }

    if (_loopCts.IsCancellationRequested)
    {
      await OnCancelledAsync(ex: null, CancellationToken.None);
    }
    else
    {
      await OnCompletedAsync(_loopCts.Token);
    }
  }

  protected abstract Task ProcessMessagesAsync(CancellationToken cancelToken = default);

  public void Initialize(ChannelSettings settings, Channel<IMessage> channel)
  {
    _channel = channel;
    _settings = settings;
  }

  protected async Task OnExceptionAsync(
    IMessage message,
    Exception exception,
    CancellationToken cancelToken = default
  )
  {
    try
    {
      await consumer.OnExceptionAsync(message, exception, cancelToken)
        .ConfigureAwait(continueOnCapturedContext: false);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred during exception handling.");
    }
  }

  private async Task OnStartAsync(CancellationToken cancelToken = default)
  {
    try
    {
      await consumer.OnStartAsync(cancelToken);
    }
    catch (Exception exInner)
    {
      logger.LogError(exInner, "An error occurred during channel worker start.");
    }
  }

  private async Task OnCompletedAsync(CancellationToken cancelToken = default)
  {
    try
    {
      await consumer.OnCompletedAsync(cancelToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred during channel completion.");
    }
  }

  private async Task OnCancelledAsync(Exception? ex, CancellationToken cancelToken = default)
  {
    try
    {
      await consumer.OnCancelledAsync(ex, cancelToken);
    }
    catch (Exception exInner)
    {
      logger.LogError(exInner, "An error occurred during channel completion.");
    }
  }
}

internal partial class ChannelWorker<TConsumer, TMessage>(ILogger<TConsumer> logger, TConsumer consumer)
  : ChannelWorker(logger, consumer)
  where TConsumer : IMessageConsumer<TMessage>
  where TMessage : IMessage
{
  private readonly ILogger _logger = logger;
  private TConsumer _consumer = consumer;

  private static string ExpectedMessageTypeName => typeof(TMessage).FullName ?? "unknown";

  [LoggerMessage(
    Level = LogLevel.Trace,
    Message =
      "Received a message expecting type {ExpectedTypeName}, but received: {Actual:Name}. Dropping message."
  )]
  public partial void LogDroppedMessage(string ExpectedTypeName, IMessage Actual);

  protected async override Task ProcessMessagesAsync(CancellationToken cancelToken = default)
  {
    CancellationTokenSource iterationCts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);

    try
    {
      while (await Channel.Reader.WaitToReadAsync(cancelToken)
               .ConfigureAwait(continueOnCapturedContext: false))
      {
        if (cancelToken.IsCancellationRequested)
        {
          return;
        }

        IMessage message = default(TMessage)!;

        try
        {
          message = await Channel.Reader.ReadAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);

          if (message is not TMessage)
          {
            LogDroppedMessage(ExpectedMessageTypeName, message);

            continue; // Do not return here - that kills the worker (╯°□°）╯︵ ┻━┻
          }

          if (_consumer.IsSubscribedTo(message))
          {
            if (!iterationCts.TryReset())
            {
              logger.LogWarning(
                "Channel worker could not reset cancellation token source. Creating new one."
              );

              iterationCts.Dispose();
              iterationCts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
            }

            await _consumer.OnMessageAsync(message, iterationCts.Token)
              .ConfigureAwait(continueOnCapturedContext: false);
          }
        }
        catch (ChannelClosedException ex)
        {
          logger.LogDebug(ex, "Channel was closed.");
        }
        catch (OperationCanceledException ex)
        {
          logger.LogDebug(ex, "Operation was cancelled.");
        }
        catch (Exception ex)
        {
          await OnExceptionAsync(message, ex, cancelToken);
        }
      }
    }
    finally
    {
      iterationCts.Dispose();
    }
  }
}