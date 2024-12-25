using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Api.Transformations.CliParser;

public class ScopeLogDrain : IParserLogger
{
    public List<string> Messages { get; } = new();
    
    public void Log(object message)
    {
        Messages.Add(message.ToString() ?? string.Empty);
    }
}