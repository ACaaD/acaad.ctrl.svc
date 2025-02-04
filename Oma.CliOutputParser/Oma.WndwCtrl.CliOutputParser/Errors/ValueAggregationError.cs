namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record ValueAggregationError : ParserStateError
{
  protected ValueAggregationError() : base(isExceptional: true)
  {
  }
}