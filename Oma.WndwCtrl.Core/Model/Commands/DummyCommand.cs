namespace Oma.WndwCtrl.Core.Model.Commands;

[Serializable]
public class DummyCommand : BaseCommand
{
  public IEnumerable<string> Returns { get; set; } = [];

  public bool SimulateFailure { get; set; } = false;

  public bool IsExceptional { get; set; } = false;

  public bool IsExpected { get; set; } = false;
}