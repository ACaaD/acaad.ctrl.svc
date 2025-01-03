namespace Oma.WndwCtrl.Core.Model.Triggers;

public record EventBasedTrigger : BaseTrigger
{
  public string? Type { get; init; }
  public string? Topic { get; init; }
  public string? Name { get; init; }
  public string? Match { get; init; }
}