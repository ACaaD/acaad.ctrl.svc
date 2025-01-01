namespace Oma.WndwCtrl.Core.Model.Transformations;

[Serializable]
public class ParserTransformation : BaseTransformation
{
  public IReadOnlyList<string> Statements { get; init; } = [];
}