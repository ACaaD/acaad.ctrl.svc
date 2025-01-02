using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Api;

public class CtrlApiProgram
{
  public async static Task Main(string[] args)
  {
    ILoggerFactory loggerFactory =
      LoggerFactory.Create(builder => { builder.SetMinimumLevel(LogLevel.Trace); });

    MessageBusAccessor messageBusAccessor = new();

    CtrlApiService apiService = new(
      loggerFactory.CreateLogger<CtrlApiService>(),
      await ComponentConfigurationAccessor.FromFileAsync(),
      messageBusAccessor // TODO: This will crash in stand alone
    );

    await apiService.StartAsync(CancellationToken.None, args);
    await apiService.WaitForShutdownAsync(CancellationToken.None);
  }
}