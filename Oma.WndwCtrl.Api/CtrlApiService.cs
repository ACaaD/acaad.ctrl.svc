using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Api.Conventions;
using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.Api.Hubs;
using Oma.WndwCtrl.Api.OpenApi;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;

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

  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    base
      .ConfigureServices(services)
      .UseMessageBus(_messageBusAccessor)
      .AddMessageConsumer<EventHub, Event>(registerConsumer: true)
      .AddComponentApi()
      .AddOpenApiComponentWriters()
      .AddSingleton(configurationAccessor)
      .AddSingleton(_messageBusAccessor).AddOpenApi(
        options =>
        {
          options.AddDocumentTransformer(
            (document, _, _) =>
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

    services.AddSignalR();

    return services;
  }

  protected override WebApplication PostAppBuild(WebApplication app)
  {
    // TODO/TBD: How to apply versioning? 
    app.MapHub<EventHub>("/events");
    return base.PostAppBuild(app);
  }

  protected override WebApplication PostAppRun(
    WebApplication application,
    CancellationToken cancelToken = default
  )
  {
    application.Services.StartConsumersAsync(
      _messageBusAccessor.MessageBus ?? throw new InvalidOperationException("MessageBus is not populated."),
      cancelToken
    );

    return base.PostAppRun(application, cancelToken);
  }
}