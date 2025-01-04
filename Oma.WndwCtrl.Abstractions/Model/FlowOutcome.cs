using System.Text.Json;

namespace Oma.WndwCtrl.Abstractions.Model;

public record FlowOutcome : IOutcome
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

  public bool Success { get; init; }
  public string OutcomeRaw { get; init; } = string.Empty;
}

[Serializable]
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

  public TData? Outcome { get; init; }
}