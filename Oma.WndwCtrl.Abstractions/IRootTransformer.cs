using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

public interface IRootTransformer
{
  Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
    ICommand command,
    Either<FlowError, CommandOutcome> commandOutcome,
    CancellationToken cancelToken = default
  );
}