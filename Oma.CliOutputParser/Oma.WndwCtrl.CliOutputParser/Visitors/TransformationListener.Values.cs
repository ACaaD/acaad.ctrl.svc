namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
    public override void ExitValuesAvg(Grammar.CliOutputParser.ValuesAvgContext context)
    {
        Func<IEnumerable<object>, object?> fold = val => val
            .Where(v => int.TryParse(v.ToString()!, out _))
            .Average(v => int.Parse(v.ToString()!));

        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);

        base.ExitValuesAvg(context);
    }

    public override void ExitValuesSum(Grammar.CliOutputParser.ValuesSumContext context)
    {
        Func<IEnumerable<object>, object?> fold = val => val
            .Where(v => int.TryParse(v.ToString()!, out _))
            .Sum(v => int.Parse(v.ToString()!));

        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);

        base.ExitValuesSum(context);
    }

    public override void ExitValuesFirst(Grammar.CliOutputParser.ValuesFirstContext context)
    {
        Func<IEnumerable<object>, object> fold = val =>
        {
            var result = val.First();
            return result;
        };

        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);

        base.ExitValuesFirst(context);
    }
}