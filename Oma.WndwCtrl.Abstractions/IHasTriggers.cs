using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Abstractions;

public interface IHasTriggers
{
  [JsonIgnore]
  IEnumerable<ITrigger> Triggers { get; }
}