using Oma.WndwCtrl.CliOutputParser.Grammar;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener : CliOutputParserBaseListener
{
    private readonly Action<object> _log;

    public TransformationListener(Action<object> log, string input)
    {
        _log = log;
        CurrentValues = [input];

        LogCurrentState("input");
    }
    
    public IEnumerable<object> CurrentValues { get; private set; }
    
    public override void EnterStatement(Grammar.CliOutputParser.StatementContext context)
    {
        _log($"{Environment.NewLine}\t### COMMAND -> {context.GetChild(0).GetText()}");
        base.EnterStatement(context);
    }
}
