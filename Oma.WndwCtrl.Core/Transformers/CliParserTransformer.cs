using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Core.Transformers;

public class CliParserTransformer : IOutcomeTransformer
{
    public Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        Either<FlowError, CommandOutcome> commandOutcome, CancellationToken cancelToken = default
    )
    {
        throw new NotImplementedException();
    }
}