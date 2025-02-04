using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record ProcessingError(string ErrorMessage, int Line, int CharPositionInLine)
  : FlowError(IsExceptional: false, IsExpected: true)
{
  public override string Message => ErrorMessage;
}

public record ProcessingError<TType>(string Message, int Line, int CharPositionInLine, TType OffendingSymbol)
  : ProcessingError(Message, Line, CharPositionInLine)
{
  public override string ToString() => $"[{Line}:{CharPositionInLine}] {Message} - {OffendingSymbol}";
}