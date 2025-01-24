using Oma.WndwCtrl.CliOutputParser.Grammar;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.CliOutputParser.Model;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener : CliOutputParserBaseListener
{
  private readonly IParserLogger _log;

  public TransformationListener(IParserLogger log, IEnumerable<object> values)
  {
    _log = log;
    CurrentValues = NestedEnumerable.FromEnumerable(values);

    LogCurrentState("input");
  }

  public TransformationListener(IParserLogger log, string input)
  {
    _log = log;
    CurrentValues = NestedEnumerable.FromString(input);

    LogCurrentState("input");
  }

  public NestedEnumerable CurrentValues { get; private set; }

  public override void EnterStatement(Grammar.CliOutputParser.StatementContext context)
  {
    if (_log.Enabled)
    {
      _log.Log($"{Environment.NewLine}\t### COMMAND -> {context.GetChild(i: 0).GetText()}");
    }

    base.EnterStatement(context);
  }
}