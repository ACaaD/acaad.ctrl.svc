using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Triggers;

[PublicAPI]
public record RateTrigger : BaseTrigger, ISchedulableTrigger
{
  [JsonRequired]
  public string Expression { get; init; } = string.Empty;

  public override string ToString() => $"Rate({Expression})";
}