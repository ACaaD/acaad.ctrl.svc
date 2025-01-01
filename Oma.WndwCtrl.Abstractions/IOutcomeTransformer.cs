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
    Either<FlowError, TransformationOutcome> transformationOutcome,
    CancellationToken cancelToken = default
  );
}

public interface IOutcomeTransformer<in TTransformation> : IOutcomeTransformer
{
  bool IOutcomeTransformer.Handles(ITransformation transformation)
  {
    return transformation is TTransformation;
  }

  async Task<Either<FlowError, TransformationOutcome>> IOutcomeTransformer.TransformCommandOutcomeAsync(
    ITransformation transformation,
    Either<FlowError, TransformationOutcome> transformationOutcome,
    CancellationToken cancelToken
  )
  {
    if (transformation is not TTransformation castedTransformation)
    {
      return Left<FlowError>(
        new ProgrammingError($"Passed command is not of type {typeof(TTransformation).Name}", 100));
    }

    return await TransformCommandOutcomeAsync(castedTransformation, transformationOutcome, cancelToken);
  }

  Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
    TTransformation transformation,
    Either<FlowError, TransformationOutcome> transformationOutcome,
    CancellationToken cancelToken = default
  );
}