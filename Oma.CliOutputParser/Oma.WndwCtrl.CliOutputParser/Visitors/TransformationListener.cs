using System.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;
using Oma.WndwCtrl.CliOutputParser.Extensions;
using Oma.WndwCtrl.CliOutputParser.Grammar;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public class TransformationListener : CliOutputParserBaseListener
{
    private readonly Action<object> _log;

    public TransformationListener(Action<object> log, string input)
    {
        _log = log;
        CurrentValues = [input];

        LogCurrentState("input");
    }

    private void UpdateValues(IEnumerable<object> newValues)
    {
        CurrentValues = newValues;
    }

    public IEnumerable<object> CurrentValues { get; set; }

    private string LogDataRecursive(IEnumerable<object> nestedList)
    {
        string toLog = "[";

        if (nestedList is IEnumerable<IEnumerable<object>> list)
        {
            foreach (var listItem in list)
            {
                toLog += LogDataRecursive(listItem);
            }
        }

        if (nestedList is not IEnumerable<IEnumerable<object>> _)
        {
            object[]? arr = nestedList?.ToArray() ?? new object[0];
            int count = arr.Length;

            for (int i = 0; i < count; i++)
            {
                toLog += $"'{arr[i].ToString()}'";
                if (i != count - 1)
                {
                    toLog += ", ";
                }
            }
        }

        toLog += "]";
        return toLog;
    }

    private IEnumerable<object> MapItemsRecursive(IEnumerable<object> nestedList, Func<object, object> map)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> list)
        {
            return list.Select(l => MapItemsRecursive(l, map));
        }

        return nestedList.Select(map);
    }

    private IEnumerable<IEnumerable<object>> UnfoldItemsRecursive(IEnumerable<object> nestedList, Func<IEnumerable<object>, IEnumerable<IEnumerable<object>>> unfold)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(l => UnfoldItemsRecursive(l, unfold));
        }

        if (nestedList is IEnumerable<object>)
        {
            var unfoldResult = unfold(nestedList); 
            return unfoldResult;   
        }
        
        Console.WriteLine("whatsgoingon");
        throw new InvalidOperationException("abc");
    }

    private object FoldItemsRecursive(IEnumerable<object> nestedList, Func<IEnumerable<object>, object> fold)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(l => FoldItemsRecursive(l, fold));
        }

        return fold(nestedList);
    }

    private void LogCurrentState(string name)
    {
        _log($"{Environment.NewLine}After {name}:");
        _log($"JSON::{JsonSerializer.Serialize(CurrentValues)}");
        
        string toLog = LogDataRecursive(CurrentValues);
        _log($"RECU::{toLog}");
    }
    
    public override void ExitMap(Grammar.CliOutputParser.MapContext context)
    {
        LogCurrentState("map");
        base.ExitMap(context);
    }

    public override void ExitMultiply(Grammar.CliOutputParser.MultiplyContext context)
    {
        LogCurrentState("multiply");
        base.ExitMultiply(context);
    }

    public override void ExitReduce(Grammar.CliOutputParser.ReduceContext context)
    {
        LogCurrentState("reduce");
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

        Func<IEnumerable<object>, IEnumerable<IEnumerable<object>>> unfold = val =>
        {
            List<List<string>> result = new();

            foreach (var items in val)
            {
                var matches = r.Matches(items.ToString()!);

                foreach (Match match in matches)
                {
                    List<string> innerResult = new();

                    foreach (Group group in match.Groups)
                    {
                        innerResult.Add(group.ToString());
                    }
                    
                    result.Add(innerResult);
                }
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
