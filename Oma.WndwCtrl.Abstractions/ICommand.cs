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

  TimeSpan WaitOnComplete { get; }

  [JsonIgnore]
  string Category { get; }

  IEnumerable<ITransformation> Transformations { get; }

  [JsonIgnore]
  Option<IComponent> Component { get; set; }
}