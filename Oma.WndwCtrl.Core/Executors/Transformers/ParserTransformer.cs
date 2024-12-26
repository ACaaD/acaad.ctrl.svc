using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class ParserTransformer : IOutcomeTransformer<ParserTransformation>
{
    public Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        ParserTransformation transformation, Either<FlowError, CommandOutcome> commandOutcome, CancellationToken cancelToken = default
    )
    {
        throw new NotImplementedException();
    }
}