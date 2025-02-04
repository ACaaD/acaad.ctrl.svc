namespace Oma.WndwCtrl.CliOutputParser.Errors;

public record EmptyEnumerationAggregationError : ValueAggregationError
{
  private readonly string _aggregationFunction;

  public EmptyEnumerationAggregationError(string aggregationFunction)
  {
    _aggregationFunction = aggregationFunction;
  }

  public override string? Detail =>
    $"Strict aggregation (function: '{_aggregationFunction}') requires at least one value to be present, but the collection was empty. This could be caused by an invalid transformation or the input text was in an invalid/irregular format.";
}