using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.CommandProcessor.Messaging;

public class ComponentToExecuteMessageConsumer(
  ILogger<ComponentToExecuteMessageConsumer> logger,
  IMessageBusWriter messageBusWriter,
  IServiceScopeFactory serviceScopeFactory
)
  : IMessageConsumer<ComponentToRunEvent>
{
  private Guid Instance { get; } = Guid.NewGuid();
  public bool IsSubscribedTo(IMessage message) => true;

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    // TODO: Raise event 
    logger.LogError(exception, "An unexpected error occurred processing message {msg}.", message);
    return Task.CompletedTask;
  }

  public async Task OnMessageAsync(ComponentToRunEvent message, CancellationToken cancelToken = default)
  {
    await using AsyncServiceScope serviceScope = serviceScopeFactory.CreateAsyncScope();

    IFlowExecutor flowExecutor =
      serviceScope.ServiceProvider.GetRequiredKeyedService<IFlowExecutor>(ServiceKeys.AdHocFlowExecutor);

    IComponent component = message.Component;
    IList<ICommand> commands = component.Commands.ToList();

    ComponentExecutingEvent componentExecutingEvent =
      await RaiseExecutionStartingAsync(message, commands, cancelToken);

    ComponentExecutionResult executionResult = ComponentExecutionResult.Succeeded;

    foreach (ICommand command in commands)
    {
      Either<FlowError, FlowOutcome> either = await flowExecutor.ExecuteAsync(command, cancelToken);

      ComponentCommandExecutionFinished eventToRaise = either.Match<ComponentCommandExecutionFinished>(
        err => new ComponentCommandExecutionFailed(componentExecutingEvent, err),
        outcome => new ComponentCommandExecutionSucceeded(componentExecutingEvent, outcome)
      );

      await messageBusWriter.SendAsync(eventToRaise, cancelToken);
    }

    await RaiseExecutionFinishedAsync(executionResult, componentExecutingEvent, cancelToken);
  }

  private async Task RaiseExecutionFinishedAsync(
    ComponentExecutionResult executionResult,
    ComponentExecutingEvent componentExecutingEvent,
    CancellationToken cancelToken
  )
  {
    ComponentExecutedEvent componentExecutedEvent = new(
      componentExecutingEvent,
      DateTime.UtcNow,
      executionResult
    );

    await messageBusWriter.SendAsync(componentExecutedEvent, cancelToken);
  }

  private async Task<ComponentExecutingEvent> RaiseExecutionStartingAsync(
    ComponentToRunEvent message,
    IList<ICommand> commands,
    CancellationToken cancelToken
  )
  {
    ComponentExecutingEvent componentExecutingEvent = new(message, commands.Count, DateTime.UtcNow);
    await messageBusWriter.SendAsync(componentExecutingEvent, cancelToken);
    return componentExecutingEvent;
  }
}