using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Core.Errors.Commands;

public record CliCommandError : CommandError
{
  public CliCommandError(Error other) : base(other)
  {
  }

  public CliCommandError(TechnicalError technicalError) : base(technicalError)
  {
  }

  public CliCommandError(string message, bool isExceptional, bool isExpected) : base(message,
    isExceptional,
    isExpected)
  {
  }
}