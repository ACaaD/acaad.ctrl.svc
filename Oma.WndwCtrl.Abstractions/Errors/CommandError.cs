using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record CommandError : FlowError, ICommandExecutionMetadata
{
  protected CommandError(Error other) : base(other)
  {
  }

  protected CommandError(TechnicalError technicalError) : base(technicalError)
  {
  }

  protected CommandError(string message, bool isExceptional, bool isExpected) : base(message,
    isExceptional,
    isExpected)
  {
  }

  public override Option<Error> Inner { get; } = Option<Error>.None;

  public Option<TimeSpan> ExecutionDuration { get; set; } = Option<TimeSpan>.None;
  public Option<int> ExecutedRetries { get; set; } = Option<int>.None;

  public static implicit operator CommandError(TechnicalError error)
  {
    return new CommandError(error);
  }
}