namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record ValueAggregationError : ParserStateError
{
  public ValueAggregationError() : base(isExceptional: true)
  {
  }
}