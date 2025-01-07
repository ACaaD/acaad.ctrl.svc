using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Conventions;
using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model;
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

        options.AddOperationTransformer(
          async (operation, context, arg3) =>
          {
            if (context.Description.ActionDescriptor.Properties.TryGetValue(
                  nameof(Component),
                  out object? componentObj
                )
                && componentObj is IComponent component)
            {
              IOpenApiExtension acaadExtension = new OpenApiObject()
              {
                ["type"] = new OpenApiString(component.Type),
                ["name"] = new OpenApiString(component.Name),
              };

              operation.Extensions.Add("acaad", acaadExtension);
            }
          }
        );
      }
    );
}