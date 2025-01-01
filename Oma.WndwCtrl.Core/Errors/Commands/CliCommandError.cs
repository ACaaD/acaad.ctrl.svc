using JetBrains.Annotations;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Core.Errors.Commands;

public record CliCommandError : CommandError
{
  [PublicAPI]
  public CliCommandError(Error other) : base(other)
  {
  }

  [PublicAPI]
  public CliCommandError(TechnicalError technicalError) : base(technicalError)
  {
  }

  [PublicAPI]
  public CliCommandError(string message, bool isExceptional, bool isExpected) : base(
    message,
    isExceptional,
    isExpected
  )
  {
  }
}