using LanguageExt;
using static LanguageExt.Prelude;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;

namespace Oma.WndwCtrl.Core.FlowExecutors;

public class NoOpFlowExecutor : IFlowExecutor
{
    public Task<Either<FlowError, TransformationOutcome>> ExecuteAsync(
        ICommand command, CancellationToken cancelToken = default
    ) => Task.FromResult<Either<FlowError, TransformationOutcome>>(Right(new TransformationOutcome()));
}