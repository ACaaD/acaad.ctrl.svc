using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Model;

[PublicAPI]
[MustDisposeResource]
public record FlowOutcome : IOutcome, IDisposable
{
  public FlowOutcome()
  {
  }

  protected FlowOutcome(bool success, string outcomeRaw)
  {
    Success = success;
    OutcomeRaw = outcomeRaw;
  }

  public FlowOutcome(IOutcome outcome)
  {
    Success = outcome.Success;
    OutcomeRaw = outcome.OutcomeRaw;
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  public bool Success { get; init; }

  [JsonInclude]
  public string OutcomeRaw { get; internal set; } = string.Empty;

  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      OutcomeRaw = null!;
    }
  }
}

[Serializable]
[MustDisposeResource]
public record FlowOutcome<TData> : FlowOutcome
{
  public FlowOutcome()
  {
    // Json constructor
  }

  public FlowOutcome(IOutcome outcome) : base(outcome)
  {
  }

  public FlowOutcome(TData data, bool success = true)
    : base(success, JsonSerializer.Serialize(data))
  {
    Outcome = data;
  }

  [JsonInclude]
  public TData? Outcome { get; internal set; }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      Outcome = default;
    }

    base.Dispose(disposing);
  }
}