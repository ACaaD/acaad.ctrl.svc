using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Core.FlowExecutors;

public class AdHocFlowExecutor : IFlowExecutor
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IOutcomeTransformer _outcomeTransformer;

    public AdHocFlowExecutor(
        [FromKeyedServices(ServiceKeys.EntryExecutor)] ICommandExecutor commandExecutor, 
        IOutcomeTransformer outcomeTransformer
    )
    {
        _commandExecutor = commandExecutor;
        _outcomeTransformer = outcomeTransformer;
    }

    public async Task<Either<FlowError, TransformationOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default)
    {
        Either<FlowError, TransformationOutcome> result = await _commandExecutor.ExecuteAsync(command, cancelToken: cancelToken)
            .BindAsync(oc => _outcomeTransformer.TransformCommandOutcomeAsync(oc, cancelToken));

        return result;
    }
}