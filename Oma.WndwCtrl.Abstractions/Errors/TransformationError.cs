using JetBrains.Annotations;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record TransformationError : FlowError
{
  public TransformationError(Error other) : base(other)
  {
  }

  [PublicAPI]
  public TransformationError(string message, bool isExceptional, bool isExpected) : base(
    isExceptional
  )
  {
  }
}