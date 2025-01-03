using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Triggers;

public record RateTrigger : BaseTrigger, ISchedulableTrigger
{
  [JsonRequired]
  public string Expression { get; init; } = string.Empty;

  public override string ToString() => $"Rate({Expression})";
}