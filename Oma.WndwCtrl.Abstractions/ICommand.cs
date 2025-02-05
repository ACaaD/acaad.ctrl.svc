using System.Text.Json.Serialization;
using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions.Model;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

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

  IList<ITransformation> Transformations { get; }

  [JsonIgnore]
  Option<IComponent> Component { get; set; }

  ValueType InferredType => Transformations.LastOrDefault()?.ValueType ?? default;

  Cardinality InferredCardinality => Transformations.LastOrDefault()?.Cardinality ?? default;
}