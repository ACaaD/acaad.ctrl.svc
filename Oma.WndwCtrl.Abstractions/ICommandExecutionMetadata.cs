using JetBrains.Annotations;
using LanguageExt;

namespace Oma.WndwCtrl.Abstractions;

public interface ICommandExecutionMetadata
{
  [UsedImplicitly]
  Option<TimeSpan> ExecutionDuration { get; }

  [UsedImplicitly]
  Option<int> ExecutedRetries { get; }
}