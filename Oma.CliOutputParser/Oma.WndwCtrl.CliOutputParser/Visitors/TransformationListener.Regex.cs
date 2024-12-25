using System.Text.RegularExpressions;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
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
        int index = int.Parse(context.INT().GetText());

        Func<IEnumerable<object>, object?> fold = val =>
        {
            var itemList = val.ToList();

            if (index > itemList.Count - 1)
            {
                return null;
            }

            return itemList[index];
        };

        var result = FoldItemsRecursive(CurrentValues, fold);
        StoreFoldResult(result);

        base.ExitRegexYield(context);
    }
}