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
    
    private Func<object, object>? _currentMap = val => val;
    private Func<object, IEnumerable<object>>? _currentUnfold;
    private Func<IEnumerable<object>, object>? _currentFold;
    
    private void UpdateValues(IEnumerable<object> newValues)
    {
        CurrentValues = newValues;
    }
    
    private IEnumerable<object> IterateItemsRecursive(IEnumerable<object> nestedList)
    {
        if (_currentMap is null)
        {
            throw new InvalidOperationException($"{nameof(_currentMap)} is null"); 
        }
        
        if (nestedList is IEnumerable<IEnumerable<object>> list)
        {
            return list.Select(IterateItemsRecursive);
        }
        
        return nestedList.Select(_currentMap);
    }
    
    public IEnumerable<object> CurrentValues { get; set; }
    public string currentUnfold;
    private IEnumerable<object> UnfoldItemsRecursive(IEnumerable<object> nestedList)
    {
        if (_currentUnfold is null)
        {
            throw new InvalidOperationException($"{nameof(_currentUnfold)} is null");
        }
        
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(UnfoldItemsRecursive);
        }

        return nestedList.Select(_currentUnfold);
    }
    
    private object FoldItemsRecursive(IEnumerable<object> nestedList)
    {
        if (_currentFold is null)
        {
            throw new InvalidOperationException($"{nameof(_currentFold)} is null");
        }

        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(FoldItemsRecursive).AsEnumerable();
        }

        return _currentFold(nestedList);
    }
    
    public override void ExitMap(Grammar.CliOutputParser.MapContext context)
    {
        var newValues = IterateItemsRecursive(CurrentValues);
        _currentMap = v => v; // TODO: grammar wrong?
        
        UpdateValues(newValues);    
        base.ExitMap(context);
    }

    public override void ExitMultiply(Grammar.CliOutputParser.MultiplyContext context)
    {
        var newValues = UnfoldItemsRecursive(CurrentValues).ToList().AsEnumerable();
        _currentUnfold = null;
        
        UpdateValues(newValues);    
        base.ExitMultiply(context);
    }

    public override void ExitReduce(Grammar.CliOutputParser.ReduceContext context)
    {
        var newValues = FoldItemsRecursive(CurrentValues);
        
        if (newValues is IEnumerable<object> newVal)
        {
            UpdateValues(newVal.ToList().AsEnumerable());       
        }
        else
        {
            Console.WriteLine("help");
        }
        
        _currentFold = null;
        
        base.ExitReduce(context);
    }

    public override void ExitAnchorFrom(Grammar.CliOutputParser.AnchorFromContext context)
    {
        string from = context.STRING_LITERAL().GetText().Trim('"');

        _currentMap = val =>
        {
            var newVal = val.ToString()!.From(from);
            return newVal;
        };
        
        base.ExitAnchorFrom(context);
    }

    public override void ExitAnchorTo(Grammar.CliOutputParser.AnchorToContext context)
    {
        string to = context.STRING_LITERAL().GetText().Trim('"');

        _currentMap = val => val.ToString()!.To(to);
        
        base.ExitAnchorTo(context);
    }

    public override void ExitRegexMatch(Grammar.CliOutputParser.RegexMatchContext context)
    {
        string pattern = context.REGEX_LITERAL().GetText().Trim('$').Trim('"');
        Regex r = new(pattern, RegexOptions.Multiline);

        currentUnfold = pattern;
        
        _currentUnfold = val =>
        {
            List<string> result = new();
            var matches = r.Matches(val.ToString()!);

            foreach (var match in matches)
            {
                result.Add(match.ToString()!);
            }
            return result;
        };
        
        base.EnterRegexMatch(context);
    }

    public override void ExitRegexYield(Grammar.CliOutputParser.RegexYieldContext context)
    {
        // TODO: FIX (First instead of Average)
        _currentFold = val => val.First();
        base.ExitRegexYield(context);
    }

    public override void ExitValuesAvg(Grammar.CliOutputParser.ValuesAvgContext context)
    {
        // TODO: FIX (First instead of Average)
        _currentFold = val => val.First();
        base.ExitValuesAvg(context);
    }

    public override void ExitValuesFirst(Grammar.CliOutputParser.ValuesFirstContext context)
    {
        _currentFold = val =>
        {
            var result = val.First();
            return result;
        };
        base.ExitValuesFirst(context);
    }
}