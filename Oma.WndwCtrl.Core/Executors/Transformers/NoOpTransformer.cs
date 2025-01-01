using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class NoOpTransformer : IOutcomeTransformer<NoOpTransformation>
{
  public Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
    NoOpTransformation transformation,
    Either<FlowError, TransformationOutcome> transformationOutcome,
    CancellationToken cancelToken = default
  )
  {
    return Task.FromResult(transformationOutcome);
  }
}