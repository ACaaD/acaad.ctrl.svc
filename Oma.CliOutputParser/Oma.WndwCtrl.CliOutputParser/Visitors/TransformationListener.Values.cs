namespace Oma.WndwCtrl.CliOutputParser.Visitors;

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
      return val.Where(v => int.TryParse(v.ToString()!, out var _))
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
      return val.Where(v => int.TryParse(v.ToString()!, out var _))
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
      return val.Where(v => int.TryParse(v.ToString()!, out var _))
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
      return val.Where(v => int.TryParse(v.ToString()!, out var _))
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
      List<object>? itemList = val.ToList();

      return index > itemList.Count - 1
        ? null
        : itemList[index];
    }
  }
}