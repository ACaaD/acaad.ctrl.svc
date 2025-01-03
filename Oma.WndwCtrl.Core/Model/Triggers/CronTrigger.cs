using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Triggers;

[Serializable]
[PublicAPI]
public record CronTrigger : BaseTrigger, ISchedulableTrigger
{
  [JsonRequired]
  public string Expression { get; init; } = string.Empty;
}