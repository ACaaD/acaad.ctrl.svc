using System.Text.Json;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
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

  private static string LogDataRecursive(IEnumerable<object> nestedList)
  {
    string toLog = "[";

    if (nestedList is IEnumerable<IEnumerable<object>> list)
    {
      toLog = list.Aggregate(toLog, (current, listItem) => current + LogDataRecursive(listItem));
    }

    object[] arr = nestedList.ToArray();
    int count = arr.Length;

    for (int i = 0; i < count; i++)
    {
      toLog += $"'{arr[i]}'";

      if (i != count - 1)
      {
        toLog += ", ";
      }
    }

    toLog += "]";
    return toLog;
  }
}