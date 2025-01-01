using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Api.Transformations.CliParser;

public class ScopeLogDrain : IParserLogger
{
#if DEBUG
  private readonly Lock _lock = new();
#endif

  public List<string> Messages { get; } = [];

  public void Log(object message)
  {
#if DEBUG
    _lock.Enter();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    _lock.Exit();
#endif

    Messages.Add(message.ToString() ?? string.Empty);
  }
}