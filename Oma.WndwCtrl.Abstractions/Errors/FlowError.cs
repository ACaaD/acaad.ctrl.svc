using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

[method: PublicAPI]
public record FlowError(string Message, bool IsExceptional, bool IsExpected) : Error
{
  protected FlowError(Error other) : this(other.Message, other.IsExceptional, other.IsExpected)
  {
    Code = other.Code;
    Inner = other;
  }

  public FlowError(TechnicalError technicalError) : this((Error)technicalError)
  {
    Inner = technicalError.Inner;
  }

  [PublicAPI]
  public FlowError(string message, bool isExceptional) : this(message, isExceptional, !isExceptional)
  {
  }

  public override int Code { get; }
  public override string Message { get; } = Message;
  public override bool IsExceptional { get; } = IsExceptional;
  public override bool IsExpected { get; } = IsExpected;

  public override Option<Error> Inner { get; } = Option<Error>.None;

  public override ErrorException ToErrorException() =>
    Inner.Match(
      err => new WrappedErrorExceptionalException(err),
      new WrappedErrorExceptionalException(this)
    );

  [System.Diagnostics.Contracts.Pure]
  public static FlowError NoCommandExecutorFound(ICommand command) => new(
    $"No command executor found that handles transformation type {command.GetType().FullName}.",
    isExceptional: false
  );

  [System.Diagnostics.Contracts.Pure]
  public static FlowError NoTransformerFound(ITransformation transformation) => new(
    $"No transformation executor found that handles transformation type {transformation.GetType().FullName}.",
    isExceptional: false
  );

  public static implicit operator FlowError(TechnicalError error) => new(error);
}