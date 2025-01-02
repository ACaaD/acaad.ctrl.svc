using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Conventions;
using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Api;

public class CtrlApiService(
  ILogger<CtrlApiService> logger,
  ComponentConfigurationAccessor configurationAccessor,
  MessageBusAccessor messageBusAccessor
)
  : WebApplicationWrapper<CtrlApiService>, IApiService
{
  private readonly ILogger _logger = logger;

  public static object Exposes { get; }

  public Task ForceStopAsync(CancellationToken cancelToken)
  {
    if (Application is not null)
    {
      return Application.StopAsync(cancelToken);
    }

    _logger.LogWarning("Force stop called without app running.");
    return Task.CompletedTask;
  }

  protected override MvcOptions PreConfigureMvcOptions(MvcOptions options)
  {
    options.Conventions.Add(new ComponentApplicationConvention(configurationAccessor));
    return base.PreConfigureMvcOptions(options);
  }

  protected override IServiceCollection ConfigureServices(IServiceCollection services) => base
    .ConfigureServices(services)
    .AddComponentApi()
    .AddSingleton(configurationAccessor)
    .AddSingleton(messageBusAccessor);
}