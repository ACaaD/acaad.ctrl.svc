using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public abstract record TransformationError : FlowError
{
  protected TransformationError(Error error) : base(error)
  {
  }

  protected TransformationError(bool isExceptional) : base(
    isExceptional
  )
  {
  }

  public override string Message => "An error occurred executing an outcome transformation.";
}