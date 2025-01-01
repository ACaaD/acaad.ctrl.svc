using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record FlowError : Error
{
  protected FlowError(Error other) : this(other.Message, other.IsExceptional, other.IsExpected)
  {
    Code = other.Code;
    Inner = other;
  }

  protected FlowError(TechnicalError technicalError) : this((Error)technicalError)
  {
  }

  [PublicAPI]
  public FlowError(string message, bool isExceptional) : this(message, isExceptional, !isExceptional)
  {
  }

  [PublicAPI]
  public FlowError(string message, bool isExceptional, bool isExpected)
  {
    Message = message;
    IsExceptional = isExceptional;
    IsExpected = isExpected;
  }

  public override int Code { get; }
  public override string Message { get; }
  public override bool IsExceptional { get; }
  public override bool IsExpected { get; }

  public override Option<Error> Inner { get; } = Option<Error>.None;

  public override ErrorException ToErrorException()
  {
    // TODO
    throw new NotImplementedException();
  }

  [System.Diagnostics.Contracts.Pure]
  public static FlowError NoCommandExecutorFound(ICommand command)
  {
    return new FlowError(
      $"No transformation executor found that handles transformation type {command.GetType().FullName}.",
      isExceptional: false
    );
  }

  [System.Diagnostics.Contracts.Pure]
  public static FlowError NoTransformerFound(ITransformation transformation)
  {
    return new FlowError(
      $"No transformation executor found that handles transformation type {transformation.GetType().FullName}.",
      isExceptional: false
    );
  }

  public static implicit operator FlowError(TechnicalError error)
  {
    return new FlowError(error);
  }
}