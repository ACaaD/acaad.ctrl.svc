using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Configuration.Model;

namespace Oma.WndwCtrl.Api;

public class CtrlApiProgram
{
  public async static Task Main(string[] args)
  {
    ILoggerFactory loggerFactory =
      LoggerFactory.Create(builder => { builder.SetMinimumLevel(LogLevel.Trace); });

    Lazy<IMessageBus> _ = new();

    CtrlApiService apiService = new(
      loggerFactory.CreateLogger<CtrlApiService>(),
      await ComponentConfigurationAccessor.FromFileAsync(),
      _
    );

    await apiService.StartAsync(CancellationToken.None, args);
    await apiService.WaitForShutdownAsync(CancellationToken.None);
  }
}