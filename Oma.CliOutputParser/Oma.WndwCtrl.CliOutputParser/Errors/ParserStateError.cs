using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Errors;

public abstract record ParserStateError : FlowError
{
  protected ParserStateError(bool isExceptional) : base(isExceptional)
  {
  }

  public override string Message =>
    "The CLI parser was in an invalid state during the transformation and could not proceed.";

  public Exception? ThrownException { get; init; }

  public override Exception ToException() => ThrownException ?? ToErrorException();
}