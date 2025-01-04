using System.Text.Json;

namespace Oma.WndwCtrl.Abstractions.Model;

[Serializable]
public record TransformationOutcome : IOutcome
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

  public bool Success { get; init; }
  public string OutcomeRaw { get; init; } = string.Empty;

  public virtual FlowOutcome ToFlowOutcome() => new(this);
}

[Serializable]
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

  public TData? Outcome { get; init; }

  public override FlowOutcome ToFlowOutcome() => Outcome is not null
    ? new FlowOutcome<TData>(Outcome, Success)
    : new FlowOutcome<TData>(this);
}