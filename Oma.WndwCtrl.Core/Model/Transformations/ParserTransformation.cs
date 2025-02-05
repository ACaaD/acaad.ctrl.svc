namespace Oma.WndwCtrl.Core.Model.Transformations;

[Serializable]
public record ParserTransformation : BaseTransformation
{
  public IReadOnlyList<string> Statements { get; init; } = [];
}