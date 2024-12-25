using Oma.WndwCtrl.CliOutputParser.Extensions;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
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
}