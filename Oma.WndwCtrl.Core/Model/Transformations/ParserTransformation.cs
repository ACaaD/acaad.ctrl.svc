using Oma.WndwCtrl.Abstractions.Model;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

namespace Oma.WndwCtrl.Core.Model.Transformations;

[Serializable]
public record ParserTransformation : BaseTransformation
{
  public IReadOnlyList<string> Statements { get; init; } = [];

  public Cardinality Cardinality { get; init; } = Cardinality.Single;

  public ValueType ValueType { get; init; } = ValueType.String;
}