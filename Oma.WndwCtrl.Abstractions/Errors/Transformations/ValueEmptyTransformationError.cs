namespace Oma.WndwCtrl.Abstractions.Errors.Transformations;

public record ValueEmptyTransformationError(ValueType ExpectedValueType)
  : TransformationError(isExceptional: false)
{
  public override string Message =>
    $"Expected value of type '{ExpectedValueType}' but the outcome was a null reference or empty.";
}