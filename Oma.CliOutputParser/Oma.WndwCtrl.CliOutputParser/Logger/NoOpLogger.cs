using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.CliOutputParser.Logger;

public class NoOpLogger : IParserLogger
{
  public bool Enabled => false;

  public void Log(object message)
  {
  }
}