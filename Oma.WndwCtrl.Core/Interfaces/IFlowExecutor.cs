using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Core.Interfaces;

public interface IFlowExecutor
{
  Task<Either<FlowError, TransformationOutcome>> ExecuteAsync(
    ICommand command,
    CancellationToken cancelToken = default
  );
}