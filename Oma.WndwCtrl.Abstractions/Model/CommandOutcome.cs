using System.Text.Json.Serialization;
using JetBrains.Annotations;
using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Model;

[Serializable]
[MustDisposeResource]
public sealed record CommandOutcome : ICommandExecutionMetadata, IOutcome, IDisposable
{
  public CommandOutcome()
  {
  }

  public CommandOutcome(string outcome)
  {
    Outcome = outcome;
    OutcomeRaw = outcome;
  }

  [UsedImplicitly]
  public object? Outcome { get; set; }

  public Option<TimeSpan> ExecutionDuration { get; set; } = Option<TimeSpan>.None;
  public Option<int> ExecutedRetries { get; set; } = Option<int>.None;

  public void Dispose()
  {
    Outcome = null;
    OutcomeRaw = string.Empty;
  }

  public bool Success { get; set; }

  [JsonInclude]
  public string OutcomeRaw { get; internal set; } = string.Empty;
}