using Oma.WndwCtrl.CliOutputParser.Extensions;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
  public override void ExitAnchorFrom(Grammar.CliOutputParser.AnchorFromContext context)
  {
    string from = context.STRING_LITERAL().GetText().Trim('\'');

    CurrentValues = MapItemsRecursive(CurrentValues, Map);

    base.ExitAnchorFrom(context);
    return;

    object Map(object val)
    {
      string newVal = val.ToString()!.From(from);
      return newVal;
    }
  }

  public override void ExitAnchorTo(Grammar.CliOutputParser.AnchorToContext context)
  {
    string to = context.STRING_LITERAL().GetText().Trim('\'');

    CurrentValues = MapItemsRecursive(CurrentValues, Map);

    base.ExitAnchorTo(context);
    return;

    object Map(object val)
    {
      return val.ToString()!.To(to);
    }
  }
}