using JetBrains.Annotations;
using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Model;

[Serializable]
public record CommandOutcome : ICommandExecutionMetadata, IOutcome
{
  [UsedImplicitly]
  public object? Outcome { get; set; }

  public Option<TimeSpan> ExecutionDuration { get; set; } = Option<TimeSpan>.None;
  public Option<int> ExecutedRetries { get; set; } = Option<int>.None;
  public bool Success { get; set; }
  public string OutcomeRaw { get; set; } = string.Empty;
}