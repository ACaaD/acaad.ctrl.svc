using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Api;

public class CtrlApiProgram
{
  public async static Task Main(string[] args)
  {
    MessageBusAccessor messageBusAccessor = new()
    {
      MessageBus = new NoOpMessageBus(),
    };

    CtrlApiService apiService = new(
      await ComponentConfigurationAccessor.FromFileAsync(),
      messageBusAccessor
    );

    await apiService.StartAsync(CancellationToken.None, args);
    await apiService.WaitForShutdownAsync(CancellationToken.None);
  }
}