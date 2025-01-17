using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.FlowExecutors;

public class NoOpFlowExecutor : IFlowExecutor
{
  [MustDisposeResource]
  [SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Must be disposed by caller."
  )]
  [SuppressMessage("ReSharper", "NotDisposedResource", Justification = "Method flagged as must-dispose.")]
  public Task<Either<FlowError, FlowOutcome>> ExecuteAsync(
    ICommand command,
    CancellationToken cancelToken = default
  ) => Task.FromResult<Either<FlowError, FlowOutcome>>(
    Right<FlowOutcome>(
      new FlowOutcome<ICommand>(command)
    )
  );
}