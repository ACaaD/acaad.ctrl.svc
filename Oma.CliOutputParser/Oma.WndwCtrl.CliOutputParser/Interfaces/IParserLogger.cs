namespace Oma.WndwCtrl.CliOutputParser.Interfaces;

public interface IParserLogger
{
  bool Enabled { get; }

  void Log(object message);
}