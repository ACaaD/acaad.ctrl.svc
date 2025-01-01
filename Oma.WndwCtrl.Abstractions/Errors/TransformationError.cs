using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record TransformationError : FlowError
{
  public TransformationError(Error other) : base(other)
  {
  }

  public TransformationError(string message, bool isExceptional, bool isExpected) : base(message,
    isExceptional,
    isExpected)
  {
  }
}