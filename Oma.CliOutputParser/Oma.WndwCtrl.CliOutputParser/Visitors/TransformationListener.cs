using Oma.WndwCtrl.CliOutputParser.Grammar;
using Oma.WndwCtrl.CliOutputParser.Model;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener : CliOutputParserBaseListener
{
  private readonly Action<object> _log;

  public TransformationListener(Action<object> log, IEnumerable<object> values)
  {
    _log = log;
    CurrentValues = NestedEnumerable.FromEnumerable(values);

    LogCurrentState("input");
  }

  public TransformationListener(Action<object> log, string input)
  {
    _log = log;
    CurrentValues = NestedEnumerable.FromString(input);

    LogCurrentState("input");
  }

  public NestedEnumerable CurrentValues { get; private set; }

  public override void EnterStatement(Grammar.CliOutputParser.StatementContext context)
  {
    _log($"{Environment.NewLine}\t### COMMAND -> {context.GetChild(0).GetText()}");
    base.EnterStatement(context);
  }
}