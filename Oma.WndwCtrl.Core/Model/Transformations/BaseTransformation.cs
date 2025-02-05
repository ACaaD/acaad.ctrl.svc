using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

namespace Oma.WndwCtrl.Core.Model.Transformations;

public record BaseTransformation : ITransformation
{
  public ValueType ValueType { get; init; } = ValueType.String;
  public Cardinality Cardinality { get; init; } = Cardinality.Single;
}