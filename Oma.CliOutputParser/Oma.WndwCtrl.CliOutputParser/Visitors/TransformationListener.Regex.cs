using System.Text.RegularExpressions;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
    public override void ExitRegexMatch(Grammar.CliOutputParser.RegexMatchContext context)
    {
        string pattern = context.REGEX_LITERAL().GetText().Trim('$').Trim('\'');
        Regex r = new(pattern, RegexOptions.Multiline);

        CurrentValues = UnfoldItemsRecursive(CurrentValues, Unfold);

        base.EnterRegexMatch(context);
        return;

        IEnumerable<IEnumerable<object>> Unfold(IEnumerable<object> val)
        {
            List<List<string>> result = [];

            foreach (object items in val)
            {
                MatchCollection matches = r.Matches(items.ToString()!);

                foreach (Match match in matches)
                {
                    List<string> innerResult = [];

                    foreach (Group group in match.Groups)
                    {
                        innerResult.Add(group.ToString());
                    }

                    result.Add(innerResult);
                }
            }

            return result;
        }
    }

    public override void ExitRegexYield(Grammar.CliOutputParser.RegexYieldContext context)
    {
        int index = int.Parse(context.INT().GetText());

        object? result = FoldItemsRecursive(CurrentValues, Fold);
        StoreFoldResult(result);

        base.ExitRegexYield(context);
        return;

        object? Fold(IEnumerable<object> val)
        {
            var itemList = val.ToList();

            return index > itemList.Count - 1
                ? null
                : itemList[index];
        }
    }
}