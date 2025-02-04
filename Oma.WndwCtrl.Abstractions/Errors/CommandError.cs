using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

[Serializable]
public record CommandError : FlowError, ICommandExecutionMetadata
{
  protected CommandError(Error other) : base(other)
  {
    Message = other.Message;
  }

  protected CommandError(TechnicalError technicalError) : base(technicalError)
  {
    Message = technicalError.Message;
  }

  protected CommandError(string message, bool isExceptional, bool isExpected) : base(
    isExceptional,
    isExpected
  )
  {
    Message = message;
  }

  public override string Message { get; }

  [PublicAPI]
  public Option<TimeSpan> ExecutionDuration { get; } = Option<TimeSpan>.None;

  [PublicAPI]
  public Option<int> ExecutedRetries { get; } = Option<int>.None;

  public static implicit operator CommandError(TechnicalError error) => new(error);
}