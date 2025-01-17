using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Core.Logger;

public class CliParserLoggerOptions
{
  public const string SectionName = "CliParser";

  public bool Silent { get; init; } = true;
}

public class CliParserLogger(ILogger<ICliOutputParser> logger, IOptions<CliParserLoggerOptions> options)
  : IParserLogger
{
  public void Log(object message)
  {
    if (options.Value.Silent)
    {
      return;
    }

    logger.LogTrace("{msg}", message);
  }
}