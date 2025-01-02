using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record ProcessingError(string Message, int Line, int CharPositionInLine)
  : FlowError(Message, IsExceptional: false, IsExpected: true);

public record ProcessingError<TType>(string Message, int Line, int CharPositionInLine, TType OffendingSymbol)
  : ProcessingError(Message, Line, CharPositionInLine)
{
  public override string ToString() => $"[{Line}:{CharPositionInLine}] {Message} - {OffendingSymbol}";
}