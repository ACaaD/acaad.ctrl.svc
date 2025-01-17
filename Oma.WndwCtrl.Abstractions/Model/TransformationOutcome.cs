using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Model;

[Serializable]
[MustDisposeResource]
public record TransformationOutcome : IOutcome, IDisposable
{
  public TransformationOutcome()
  {
  }

  protected TransformationOutcome(bool success, string outcomeRaw)
  {
    Success = success;
    OutcomeRaw = outcomeRaw;
  }

  public TransformationOutcome(IOutcome outcome)
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

  [MustDisposeResource]
  public virtual FlowOutcome ToFlowOutcome() => new(this);

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
public record TransformationOutcome<TData> : TransformationOutcome
{
  public TransformationOutcome()
  {
    // Json constructor
  }

  public TransformationOutcome(IOutcome outcome) : base(outcome)
  {
  }

  public TransformationOutcome(TData data, bool success = true)
    : base(success, JsonSerializer.Serialize(data))
  {
    Outcome = data;
  }

  [JsonInclude]
  public TData? Outcome { get; internal set; }

  public override FlowOutcome ToFlowOutcome() => Outcome is not null
    ? new FlowOutcome<TData>(Outcome, Success)
    : new FlowOutcome<TData>(this);

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      Outcome = default;
    }

    base.Dispose(disposing);
  }
}