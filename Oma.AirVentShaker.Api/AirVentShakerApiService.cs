using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oma.AirVentShaker.Api.Audio;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Messaging.Consumers;
using Oma.AirVentShaker.Api.Model;
using Oma.AirVentShaker.Api.Model.Events;
using Oma.AirVentShaker.Api.Model.Settings;
using Oma.AirVentShaker.Api.Sensors;
using Oma.AirVentShaker.Api.TestRunners;
using Oma.AirVentShaker.Api.Workers;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Extensions;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;

namespace Oma.AirVentShaker.Api;

public class AirVentShakerApiService(
  ComponentConfigurationAccessor configurationAccessor,
  MessageBusAccessor messageBusAccessor,
  IConfiguration rootConfiguration
)
  : WebApplicationWrapper<AirVentShakerApiService>(messageBusAccessor, rootConfiguration)
{
  private readonly MessageBusAccessor _messageBusAccessor = messageBusAccessor;

  protected override Action<JsonTypeInfo> AddAdditionalModifiers =>
    JsonExtensions.GetPolymorphismModifierFor<IWaveDescriptor>(
      t => t.Name.Replace("WaveDescriptor", string.Empty)
    );

  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    base
      .ConfigureServices(services)
      .Configure<SensorSettings>(Configuration.GetSection(SensorSettings.SectionName))
      .AddSingleton<GlobalState>()
      .AddSingleton(_messageBusAccessor)
      .UseMessageBus(_messageBusAccessor)
      .AddMessageConsumer<TimeSeriesPersistorMessageConsumer, GForceValueBatchEvent>()
      .AddMessageConsumer<AmplitudeAdjustingMessageConsumer, GForceValueBatchEvent>()
      // .AddHostedService<SensorWorker>()
      .AddHostedService<HighResSensorWorker>()
      .AddSingleton<ITestRunner, DummyTestRunner>()
      .AddSingleton<IAudioService, AudioService>();


    if (Configuration.GetValue<string>("ACaaD:OS") == "windows")
    {
      services.AddSingleton<ISensorService, DummySensorService>();
    }
    else
    {
      services.AddSingleton<ISensorService, Adxl345SensorService>();
    }

    services.AddSignalR();

    return services;
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