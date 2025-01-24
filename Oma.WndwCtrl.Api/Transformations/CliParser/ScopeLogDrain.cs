using Microsoft.Extensions.Options;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.Logger;

namespace Oma.WndwCtrl.Api.Transformations.CliParser;

public sealed class ScopeLogDrain(IOptions<CliParserLoggerOptions> options) : IParserLogger, IDisposable
{
  public bool Enabled => true;
  
  public List<string> Messages { get; } = [];

  public void Dispose()
  {
    Messages.Clear();
  }

  public void Log(object message)
  {
    if (options.Value.Silent)
    {
      return;
    }

    Messages.Add(message.ToString() ?? string.Empty);
  }
}