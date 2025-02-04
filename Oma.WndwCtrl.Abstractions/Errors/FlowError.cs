using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

[method: PublicAPI]
public record FlowError(bool IsExceptional, bool IsExpected) : Error
{
  protected FlowError(Error other) : this(other.IsExceptional, other.IsExpected)
  {
    Code = other.Code;
    Inner = other;
  }

  public FlowError(TechnicalError technicalError) : this((Error)technicalError)
  {
    Inner = technicalError.Inner;
  }

  [PublicAPI]
  public FlowError(bool isExceptional) : this(isExceptional, !isExceptional)
  {
  }

  public virtual string? Detail => Inner.Match(
    err =>
    {
      return err switch
      {
        FlowError flowError => flowError.Message,
        ManyErrors _ => "Multiple errors occurred. Refer to the nested properties for more details.",
        var _ => null,
      };
    },
    () => null
  );

  public override int Code { get; }
  public override string Message { get; } = $"An unexpected error occurred processing a flow.";
  public override bool IsExceptional { get; } = IsExceptional;
  public override bool IsExpected { get; } = IsExpected;

  public override Option<Error> Inner { get; } = Option<Error>.None;

  public override ErrorException ToErrorException() =>
    Inner.Match(
      err => new WrappedErrorExceptionalException(err),
      new WrappedErrorExceptionalException(this)
    );

  [System.Diagnostics.Contracts.Pure]
  public static FlowError NoCommandExecutorFound(ICommand command) =>
    new NoCommandExecutorFoundError(command);

  [System.Diagnostics.Contracts.Pure]
  public static FlowError NoTransformerFound(ITransformation transformation) =>
    new NoTransformerFoundError(transformation);

  public static implicit operator FlowError(TechnicalError error) => new(error);

  public record NoCommandExecutorFoundError : FlowError
  {
    private readonly string _commandType;

    public NoCommandExecutorFoundError(ICommand command) : base(isExceptional: true)
    {
      _commandType = command.GetType().FullName ?? "unknown";
    }

    public override string Message =>
      $"No command executor found that handles transformation type {_commandType}.";
  }

  public record NoTransformerFoundError : FlowError
  {
    private readonly string _transformationName;

    public NoTransformerFoundError(ITransformation transformation) : base(isExceptional: true)
    {
      _transformationName = transformation.GetType().FullName ?? "unknown";
    }

    public override string Message =>
      $"No command executor found that handles transformation type {_transformationName}.";
  }
}