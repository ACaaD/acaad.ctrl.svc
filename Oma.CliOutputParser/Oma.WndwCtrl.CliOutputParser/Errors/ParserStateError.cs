using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record ParserStateError : FlowError
{
  public ParserStateError(string message, bool isExceptional) : base(message, isExceptional)
  {
  }

  public Exception? ThrownException { get; init; }

  public override Exception ToException() => ThrownException ?? ToErrorException();
}