using Oma.WndwCtrl.Abstractions.Model;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

namespace Oma.WndwCtrl.Abstractions;

public interface ITransformation
{
  public Cardinality Cardinality { get; }

  public ValueType ValueType { get; }
}