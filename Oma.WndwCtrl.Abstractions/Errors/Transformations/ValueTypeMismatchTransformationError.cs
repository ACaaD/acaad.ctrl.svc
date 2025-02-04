namespace Oma.WndwCtrl.Abstractions.Errors.Transformations;

public record ValueTypeMismatchTransformationError(ValueType ExpectedValueType, Type actualType)
  : TransformationError(isExceptional: false)
{
  public override string Message =>
    $"Expected value of type '{ExpectedValueType}' but the outcome returned {actualType}.";
}