using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Conventions;
using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.CoreAsp;

namespace Oma.WndwCtrl.Api;

public class CtrlApiService(
  ILogger<CtrlApiService> logger,
  ComponentConfigurationAccessor configurationAccessor
)
  : WebApplicationWrapper<CtrlApiService>, IApiService
{
  private readonly ILogger _logger = logger;

  public Task ForceStopAsync(CancellationToken cancelToken)
  {
    if (Application is null)
    {
      _logger.LogWarning("Force stop called without app running.");
      return Task.CompletedTask;
    }

    return Application.StopAsync(cancelToken);
  }

  protected override MvcOptions PreConfigureMvcOptions(MvcOptions options)
  {
    options.Conventions.Add(new ComponentApplicationConvention(configurationAccessor));
    return base.PreConfigureMvcOptions(options);
  }

  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    return base
      .ConfigureServices(services)
      .AddComponentApi()
      .AddSingleton(configurationAccessor);
  }
}