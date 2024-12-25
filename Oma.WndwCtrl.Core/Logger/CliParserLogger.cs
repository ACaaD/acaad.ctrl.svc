using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Core.Logger;

public class CliParserLogger(ILogger<ICliOutputParser> logger) : IParserLogger
{
    public void Log(object message)
    {
        logger.LogTrace("{msg}", message);
    }
}