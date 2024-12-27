using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Api.Transformations.CliParser;

public class ScopeLogDrain : IParserLogger
{
    public List<string> Messages { get; } = new();
    
    public void Log(object message)
    {
#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
#endif
        Messages.Add(message.ToString() ?? string.Empty);
    }
}