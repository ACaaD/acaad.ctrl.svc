using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions.Errors.Transformations;

public record MismatchedCardinalityTransformationError(Cardinality expectedCardinality)
  : TransformationError(isExceptional: false)
{
  public override string Message =>
    $"Expected cardinality of '{expectedCardinality}' but the outcome returned is a different cardinality.";
}