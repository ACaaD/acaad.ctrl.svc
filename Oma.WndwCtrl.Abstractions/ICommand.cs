using System.Text.Json.Serialization;
using JetBrains.Annotations;
using LanguageExt;

namespace Oma.WndwCtrl.Abstractions;

public interface ICommand
{
  [UsedImplicitly]
  int Retries { get; }

  [UsedImplicitly]
  TimeSpan Timeout { get; }

  IEnumerable<ITransformation> Transformations { get; }

  [JsonIgnore]
  Option<IComponent> Component { get; set; }
}