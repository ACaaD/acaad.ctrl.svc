using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Extensions;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Core.FlowExecutors;

public class AdHocFlowExecutor(
  [FromKeyedServices(ServiceKeys.EntryCommandExecutor)]
  ICommandExecutor commandExecutor,
  [FromKeyedServices(ServiceKeys.RootTransformer)]
  IRootTransformer rootTransformer,
  IMessageBusWriter messageBusWriter
)
  : IFlowExecutor
{
  public async Task<Either<FlowError, FlowOutcome>> ExecuteAsync(
    ICommand command,
    CancellationToken cancelToken = default
  )
  {
    Either<FlowError, TransformationOutcome> either = await commandExecutor
      .ExecuteAsync(command, cancelToken)
      .BindDispose(
        oc => rootTransformer.TransformCommandOutcomeAsync(command, oc, cancelToken)
      );

    // TODO: Might be handy to add a factory here to transform (transformer specific) results
    Either<FlowError, FlowOutcome> result = either.Map(o => o.ToFlowOutcome());

    await RaiseEvent(command, result, cancelToken);

    return result;
  }

  private async Task RaiseEvent(
    ICommand command,
    Either<FlowError, FlowOutcome> either,
    CancellationToken cancelToken
  )
  {
    IComponent component = command.Component.IfNone(new AdHocComponent(command));

    Option<ComponentCommandOutcomeEvent> result =
      either.ToOption().Select(
        outcome => new ComponentCommandOutcomeEvent(component, command, outcome with { })
      );

    await result.Match<Task>(
      @event => messageBusWriter.SendAsync(@event, cancelToken),
      () => Task.CompletedTask // TODO: See if it's worth to raise an additional errored event here
    );
  }

  private record AdHocComponent(ICommand Command) : IComponent
  {
    public bool Active { get; set; } = true;
    public string Name { get; set; } = nameof(AdHocComponent);
    public string Type => "adhoc";
    public IEnumerable<ICommand> Commands => [Command,];
  }
}