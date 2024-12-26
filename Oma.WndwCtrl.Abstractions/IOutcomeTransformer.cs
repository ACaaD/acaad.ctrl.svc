using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Abstractions;

public interface IOutcomeTransformer
{
    bool Handles(ITransformation transformation);
    
    Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        ITransformation transformation,
        Either<FlowError, CommandOutcome> commandOutcome, 
        CancellationToken cancelToken = default
    );
}

public interface IOutcomeTransformer<in TTransformation> : IOutcomeTransformer
{
    bool IOutcomeTransformer.Handles(ITransformation transformation) => transformation is TTransformation;
    
    async Task<Either<FlowError, TransformationOutcome>> IOutcomeTransformer.TransformCommandOutcomeAsync(
        ITransformation transformation,
        Either<FlowError, CommandOutcome> commandOutcome, 
        CancellationToken cancelToken
    )
    {
        if (transformation is not TTransformation castedTransformation)
        {
            return Left<FlowError>(new ProgrammingError($"Passed command is not of type {typeof(TTransformation).Name}", Code: 100));
        }

        return await TransformCommandOutcomeAsync(castedTransformation, commandOutcome, cancelToken: cancelToken);
    }

    Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        TTransformation transformation,
        Either<FlowError, CommandOutcome> commandOutcome,
        CancellationToken cancelToken = default
    );
}