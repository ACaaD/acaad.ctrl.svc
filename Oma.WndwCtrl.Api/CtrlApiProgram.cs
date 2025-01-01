using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Configuration.Model;

namespace Oma.WndwCtrl.Api;

public class CtrlApiProgram
{
  public async static Task Main(string[] args)
  {
    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder.SetMinimumLevel(LogLevel.Trace);
    });

    IApiService apiService = new CtrlApiService(
      loggerFactory.CreateLogger<CtrlApiService>(),
      await ComponentConfigurationAccessor.FromFileAsync()
    );

    await apiService.StartAsync(CancellationToken.None, args);
    await apiService.WaitForShutdownAsync(CancellationToken.None);
  }
}