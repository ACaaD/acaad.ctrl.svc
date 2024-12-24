using Oma.WndwCtrl.CliOutputParser.Grammar;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public class TransformationVisitor : CliOutputParserBaseVisitor<object>
{
    public override object VisitTransformation(CliOutParser.Grammar.CliOutputParser.TransformationContext context)
    {
        return base.VisitTransformation(context);
    }
}