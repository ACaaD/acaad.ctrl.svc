namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record ValueAggregationError : ParserStateError
{
  public ValueAggregationError(string message, bool isExceptional) : base(message, isExceptional)
  {
  }
}