namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record EmptyEnumerationAggregationError : ValueAggregationError
{
  public EmptyEnumerationAggregationError(string message, bool isExceptional) : base(message, isExceptional)
  {
  }
}