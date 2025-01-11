using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Oma.WndwCtrl.Api.Conventions;
using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.Api.OpenApi;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Api;

public class CtrlApiService(
  ComponentConfigurationAccessor configurationAccessor,
  MessageBusAccessor messageBusAccessor
)
  : WebApplicationWrapper<CtrlApiService>(messageBusAccessor)
{
  private readonly MessageBusAccessor _messageBusAccessor = messageBusAccessor;

  protected override MvcOptions PreConfigureMvcOptions(MvcOptions options)
  {
    options.Conventions.Add(new ComponentApplicationConvention(configurationAccessor));
    return base.PreConfigureMvcOptions(options);
  }

  protected override IServiceCollection ConfigureServices(IServiceCollection services) => base
    .ConfigureServices(services)
    .AddComponentApi()
    .AddOpenApiComponentWriters()
    .AddSingleton(configurationAccessor)
    .AddSingleton(_messageBusAccessor).AddOpenApi(
      options =>
      {
        options.AddDocumentTransformer(
          (document, context, cancellationToken) =>
          {
            document.Info = new OpenApiInfo
            {
              Title = "Component Control API",
              Version = "v1",
              Description = "API for discovering and interacting with Components.",
            };

            return Task.CompletedTask;
          }
        );

        options.AddOperationTransformer<ComponentOperationTransformer>();
      }
    );
}