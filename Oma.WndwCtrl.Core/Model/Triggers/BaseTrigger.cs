using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Triggers;

[Serializable]
public abstract record BaseTrigger : ITrigger
{
  [JsonIgnore]
  public Guid UniqueIdentifier { get; } = Guid.NewGuid();
}