namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
    public override void ExitValuesAvg(Grammar.CliOutputParser.ValuesAvgContext context)
    {
        object? result = FoldItemsRecursive(CurrentValues, Fold);
        StoreFoldResult(result);

        base.ExitValuesAvg(context);
        return;

        object? Fold(IEnumerable<object> val) =>
            val.Where(v => int.TryParse(v.ToString()!, out _))
                .Average(v => int.Parse(v.ToString()!));
    }

    public override void ExitValuesSum(Grammar.CliOutputParser.ValuesSumContext context)
    {
        object? result = FoldItemsRecursive(CurrentValues, Fold);
        StoreFoldResult(result);

        base.ExitValuesSum(context);
        return;

        object? Fold(IEnumerable<object> val) =>
            val.Where(v => int.TryParse(v.ToString()!, out _))
                .Sum(v => int.Parse(v.ToString()!));
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
}