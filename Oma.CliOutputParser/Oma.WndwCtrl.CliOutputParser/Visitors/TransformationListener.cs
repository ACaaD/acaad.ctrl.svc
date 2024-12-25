using System.Collections;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Oma.WndwCtrl.CliOutputParser.Extensions;
using Oma.WndwCtrl.CliOutputParser.Grammar;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public class TransformationListener : CliOutputParserBaseListener
{
    public TransformationListener(string input)
    {
        CurrentValues = [input];
    }
    
    private void UpdateValues(IEnumerable<object> newValues)
    {
        CurrentValues = newValues;
    }
    
    private IEnumerable<object> MapItemsRecursive(IEnumerable<object> nestedList, Func<object, object> map)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> list)
        {
            return list.Select(l => MapItemsRecursive(l, map));
        }
        
        return nestedList.Select(map);
    }
    
    public IEnumerable<object> CurrentValues { get; set; }
    
    private IEnumerable<object> UnfoldItemsRecursive(IEnumerable<object> nestedList, Func<object, IEnumerable<object>> unfold)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(l => UnfoldItemsRecursive(l, unfold));
        }

        return nestedList.Select(unfold);
    }
    
    private object FoldItemsRecursive(IEnumerable<object> nestedList, Func<IEnumerable<object>, object> fold)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(l => FoldItemsRecursive(l, fold));
        }

        return fold(nestedList);
    }
    
    public override void ExitMap(Grammar.CliOutputParser.MapContext context)
    {
        base.ExitMap(context);
    }

    public override void ExitMultiply(Grammar.CliOutputParser.MultiplyContext context)
    {
        base.ExitMultiply(context);
    }

    public override void ExitReduce(Grammar.CliOutputParser.ReduceContext context)
    {
        base.ExitReduce(context);
    }

    public override void ExitAnchorFrom(Grammar.CliOutputParser.AnchorFromContext context)
    {
        string from = context.STRING_LITERAL().GetText().Trim('"');

        Func<object, object> map = val =>
        {
            var newVal = val.ToString()!.From(from);
            return newVal;
        };
        
        CurrentValues = MapItemsRecursive(CurrentValues, map);
        
        base.ExitAnchorFrom(context);
    }

    public override void ExitAnchorTo(Grammar.CliOutputParser.AnchorToContext context)
    {
        string to = context.STRING_LITERAL().GetText().Trim('"');

        Func<object, object> map = val => val.ToString()!.To(to);
        
        CurrentValues = MapItemsRecursive(CurrentValues, map);
        
        base.ExitAnchorTo(context);
    }

    public override void ExitRegexMatch(Grammar.CliOutputParser.RegexMatchContext context)
    {
        string pattern = context.REGEX_LITERAL().GetText().Trim('$').Trim('"');
        Regex r = new(pattern, RegexOptions.Multiline);
        
        Func<object, IEnumerable<object>> unfold = val =>
        {
            List<string> result = new();
            var matches = r.Matches(val.ToString()!);

            foreach (var match in matches)
            {
                result.Add(match.ToString()!);
            }
            return result;
        };
        
        CurrentValues = UnfoldItemsRecursive(CurrentValues, unfold);
        
        base.EnterRegexMatch(context);
    }
    
    public override void ExitRegexYield(Grammar.CliOutputParser.RegexYieldContext context)
    {
        // TODO: FIX (First instead of Average)
        Func<IEnumerable<object>, object> fold = val => val.First();
        
        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);
        
        base.ExitRegexYield(context);
    }

    public override void ExitValuesAvg(Grammar.CliOutputParser.ValuesAvgContext context)
    {
        // TODO: FIX (First instead of Average)
        Func<IEnumerable<object>, object> fold = val => val.First();
        
        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);
        
        base.ExitValuesAvg(context);
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

    private void StoreFoldResult(object result)
    {
        if (result is IEnumerable<object> results)
        {
            CurrentValues = results;
        }
        else
        {
            CurrentValues = new List<object>() { result }.AsEnumerable();
        }
    }

    public override void ExitValuesLast(Grammar.CliOutputParser.ValuesLastContext context)
    {
        Func<IEnumerable<object>, object> fold = val =>
        {
            var result = val.Last();
            return result;
        };
        
        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);
        
        base.ExitValuesLast(context);
    }
}