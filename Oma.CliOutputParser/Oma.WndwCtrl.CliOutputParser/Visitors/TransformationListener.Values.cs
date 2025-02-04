using JetBrains.Annotations;
using Oma.WndwCtrl.CliOutputParser.Errors;
using Oma.WndwCtrl.CliOutputParser.Model;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

[UsedImplicitly]
public partial class TransformationListener
{
  public override void ExitValuesAvg(Grammar.CliOutputParser.ValuesAvgContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesAvg(context);
    return;

    object? Fold(IEnumerable<object> val)
    {
      return val.Where(v => int.TryParse(v.ToString()!, out int _))
        .Average(v => int.Parse(v.ToString()!));
    }
  }

  public override void ExitValuesSum(Grammar.CliOutputParser.ValuesSumContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesSum(context);
    return;

    object? Fold(IEnumerable<object> val)
    {
      return val.Where(v => int.TryParse(v.ToString()!, out int _))
        .Sum(v => int.Parse(v.ToString()!));
    }
  }

  public override void ExitValuesMin(Grammar.CliOutputParser.ValuesMinContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesMin(context);
    return;

    object? Fold(IEnumerable<object> val)
    {
      return val.Where(v => int.TryParse(v.ToString()!, out int _))
        .Min(v => int.Parse(v.ToString()!));
    }
  }

  public override void ExitValuesMax(Grammar.CliOutputParser.ValuesMaxContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesMax(context);
    return;

    object? Fold(IEnumerable<object> val)
    {
      return val.Where(v => int.TryParse(v.ToString()!, out int _))
        .Max(v => int.Parse(v.ToString()!));
    }
  }

  public override void ExitValuesFirst(Grammar.CliOutputParser.ValuesFirstContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesFirst(context);
    return;

    object Fold(IEnumerable<object> val)
    {
      object res = val.First();
      return res;
    }
  }

  public override void ExitValuesLast(Grammar.CliOutputParser.ValuesLastContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesLast(context);
    return;

    object Fold(IEnumerable<object> val)
    {
      object res = val.Last();
      return res;
    }
  }

  public override void ExitValuesAt(Grammar.CliOutputParser.ValuesAtContext context)
  {
    int index = int.Parse(context.INT().GetText());

    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesAt(context);
    return;

    object? Fold(IEnumerable<object> val)
    {
      List<object> itemList = val.ToList();

      return index > itemList.Count - 1
        ? null
        : itemList[index];
    }
  }

  public override void ExitValuesCount(Grammar.CliOutputParser.ValuesCountContext context)
  {
    object? result = FoldItemsRecursive(CurrentValues, Fold);
    StoreFoldResult(result);

    base.ExitValuesCount(context);
    return;

    object Fold(IEnumerable<object> val)
    {
      object res = val.Count();
      return res;
    }
  }

  public override void EnterStrictValueAggregation(
    Grammar.CliOutputParser.StrictValueAggregationContext context
  )
  {
    List<object> collapsedValues = CurrentValues.ToList();

    if (collapsedValues.Count == 0)
    {
      Error = new EmptyEnumerationAggregationError("No values to average.", isExceptional: true);
      return;
    }

    CurrentValues = NestedEnumerable.FromEnumerableInternal(collapsedValues, CurrentValues.IsNested);
    base.EnterStrictValueAggregation(context);
  }
}