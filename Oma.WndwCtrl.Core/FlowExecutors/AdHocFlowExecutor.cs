using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Core.FlowExecutors;

public class AdHocFlowExecutor(
  [FromKeyedServices(ServiceKeys.EntryCommandExecutor)]
  ICommandExecutor commandExecutor,
  [FromKeyedServices(ServiceKeys.RootTransformer)]
  IRootTransformer rootTransformer
)
  : IFlowExecutor
{
  public async Task<Either<FlowError, TransformationOutcome>> ExecuteAsync(
    ICommand command,
    CancellationToken cancelToken = default
  )
  {
    Either<FlowError, TransformationOutcome> result = await commandExecutor
      .ExecuteAsync(command, cancelToken)
      .Bind(oc => rootTransformer.TransformCommandOutcomeAsync(command, oc, cancelToken));

    return result;
  }
}