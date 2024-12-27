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
    private readonly IRootTransformer _rootTransformer;

    public AdHocFlowExecutor(
        [FromKeyedServices(ServiceKeys.EntryCommandExecutor)] ICommandExecutor commandExecutor, 
        [FromKeyedServices(ServiceKeys.RootTransformer)] IRootTransformer rootTransformer
    )
    {
        _commandExecutor = commandExecutor;
        _rootTransformer = rootTransformer;
    }

    public async Task<Either<FlowError, TransformationOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default)
    {
        Either<FlowError, TransformationOutcome> result = await _commandExecutor.ExecuteAsync(command, cancelToken: cancelToken)
            .Bind(oc => _rootTransformer.TransformCommandOutcomeAsync(command, oc, cancelToken));

        return result;
    }
}