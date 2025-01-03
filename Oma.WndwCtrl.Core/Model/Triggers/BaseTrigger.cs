using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Triggers;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CronTrigger))]
[JsonDerivedType(typeof(EventBasedTrigger))]
[JsonDerivedType(typeof(RateTrigger))]
public record BaseTrigger : ITrigger
{
  [JsonIgnore]
  public Guid UniqueIdentifier { get; } = Guid.NewGuid();
}