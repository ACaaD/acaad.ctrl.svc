using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Core.Interfaces;

public interface IFlowExecutor
{
  [MustDisposeResource]
  [PublicAPI]
  Task<Either<FlowError, FlowOutcome>> ExecuteAsync(
    ICommand command,
    CancellationToken cancelToken = default
  );
}