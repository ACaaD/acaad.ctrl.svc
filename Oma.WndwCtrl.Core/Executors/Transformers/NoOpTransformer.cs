using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class NoOpTransformer : IOutcomeTransformer
{
    public Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        Either<FlowError, CommandOutcome> commandOutcome, CancellationToken cancelToken = default
    ) =>
        Task.FromResult(commandOutcome.BiBind<TransformationOutcome>(
            Right: outcome => new TransformationOutcome(outcome),
            Left: error => error
        ));
}