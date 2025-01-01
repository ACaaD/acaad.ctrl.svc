using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

[Serializable]
public record CommandError : FlowError, ICommandExecutionMetadata
{
  protected CommandError(Error other) : base(other)
  {
  }

  protected CommandError(TechnicalError technicalError) : base(technicalError)
  {
  }

  protected CommandError(string message, bool isExceptional, bool isExpected) : base(
    message,
    isExceptional,
    isExpected
  )
  {
  }

  [PublicAPI]
  public Option<TimeSpan> ExecutionDuration { get; } = Option<TimeSpan>.None;

  [PublicAPI]
  public Option<int> ExecutedRetries { get; } = Option<int>.None;

  public static implicit operator CommandError(TechnicalError error)
  {
    return new CommandError(error);
  }
}