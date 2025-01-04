namespace Oma.WndwCtrl.Abstractions;

public interface IHasCommands
{
  IEnumerable<ICommand> Commands { get; }
}