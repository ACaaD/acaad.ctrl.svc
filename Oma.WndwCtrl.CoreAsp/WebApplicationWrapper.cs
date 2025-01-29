using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Extensions;
using Oma.WndwCtrl.Core.Model.Settings;
using Oma.WndwCtrl.CoreAsp.Api.Filters;
using Oma.WndwCtrl.CoreAsp.Conventions;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;
using Scalar.AspNetCore;

namespace Oma.WndwCtrl.CoreAsp;

public class WebApplicationWrapper<TAssemblyDescriptor>(
  MessageBusAccessor? messageBusAccessor,
  IConfiguration? rootConfiguration
) : IApiService
  where TAssemblyDescriptor : class, IApiService
{
  [SuppressMessage(
    "ReSharper",
    "StaticMemberInGenericType",
    Justification = "Exactly the intended behaviour."
  )]
  private static IServiceProvider? _serviceProvider;

  private static readonly string _serviceName = typeof(TAssemblyDescriptor).Name;

  private IConfiguration? _configuration;

  private IWebHostEnvironment? _environment;

  [PublicAPI]
  protected static IServiceProvider ServiceProvider => _serviceProvider
                                                       ?? throw new InvalidOperationException(
                                                         "The WebApplicationWrapper has not been initialized properly."
                                                       );

  [PublicAPI]
  protected WebApplication? Application { get; private set; }

  protected IWebHostEnvironment Environment
  {
    get => _environment ?? throw new InvalidOperationException($"{nameof(Environment)} is not populated.");
    private set => _environment = value;
  }

  protected IConfiguration Configuration
  {
    get => _configuration ??
           throw new InvalidOperationException($"{nameof(Configuration)} is not populated.");
    private set => _configuration = value;
  }

  static IServiceProvider IService.ServiceProvider
  {
    get => ServiceProvider;
    set => _serviceProvider = value;
  }

  // TODO: Fix me
  public bool Enabled => true;

  public async Task StartAsync(CancellationToken cancelToken = default, params string[] args)
  {
#if DEBUG
    System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#endif

    if (Application is not null)
    {
      throw new InvalidOperationException("Application is already running.");
    }

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    Environment = builder.Environment;
    Configuration = builder.Configuration;

    IConfiguration coreConfig = Configuration.GetSection("Core");
    ExtensionSettings extensions = [];
    coreConfig.GetSection(ExtensionSettings.SectionName).Bind(extensions);

    ConfigurationConfiguration(builder.Configuration);

    builder.WebHost.ConfigureKestrel(
      (context, options) =>
      {
        IConfiguration configToUse = context.Configuration.GetSection(_serviceName).GetSection("Kestrel");
        options.Configure(configToUse);
      }
    );

    IMvcCoreBuilder mvcBuilder = builder.Services
      .AddMvcCore(
        opts =>
        {
          PreConfigureMvcOptions(opts);

          opts.Conventions.Add(new ContainingAssemblyApplicationModelConvention<TAssemblyDescriptor>());

          opts.Filters.Add<RequestReceivedActionFilter>();

          PostConfigureMvcOptions(opts);
        }
      )
      .AddApiExplorer()
      .AddJsonOptions(
        opts =>
        {
          ModifyJsonSerializerOptions(opts.JsonSerializerOptions);
          ConfigureJsonOptions(opts);
        }
      );

    PostConfigureMvc(mvcBuilder);

    builder.Services.AddOpenApi();

    if (messageBusAccessor is not null)
    {
      builder.Services.UseMessageBus(messageBusAccessor);
    }

    builder.Services.AddLogging(
      lb =>
      {
        // TODO: Pass down logging configuration
        lb.AddConsole();
      }
    );

    ConfigureServices(builder.Services);

    Application = PostAppBuild(builder.Build());

    TAssemblyDescriptor.ServiceProvider = Application.Services;

    Application.MapControllers();
    Application.MapOpenApi();
    Application.MapScalarApiReference();

#if DEBUG
    Application.UseDeveloperExceptionPage();
#endif

    await PreAppRun(Application).StartAsync(cancelToken);
    PostAppRun(Application, cancelToken);
  }

  public async Task WaitForShutdownAsync(CancellationToken cancelToken = default)
  {
    if (Application is not null)
    {
      try
      {
        await Application.WaitForShutdownAsync(cancelToken);
      }
      finally
      {
        await Application.DisposeAsync();
        Application = null;
      }
    }
  }

  [PublicAPI]
  public Task ForceStopAsync(CancellationToken cancelToken) => Application is not null
    ? Application.StopAsync(cancelToken)
    : Task.CompletedTask;

  public static void ModifyJsonSerializerOptions(
    JsonSerializerOptions jsonSerializerOptions
  )
  {
    jsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
      .WithAddedModifier(
        JsonExtensions.GetPolymorphismModifierFor<ICommand>(
          t => t.Name.Replace("Command", string.Empty)
        )
      )
      .WithAddedModifier(
        JsonExtensions.GetPolymorphismModifierFor<ITransformation>(
          t => t.Name.Replace("Transformation", string.Empty)
        )
      )
      .WithAddedModifier(
        JsonExtensions.GetPolymorphismModifierFor<ITrigger>(
          t => t.Name.Replace("Trigger", string.Empty)
        )
      );
  }

  [PublicAPI]
  protected virtual IConfigurationBuilder ConfigurationConfiguration(
    IConfigurationBuilder configurationBuilder
  )
  {
    if (rootConfiguration is not null)
    {
      configurationBuilder.AddConfiguration(rootConfiguration);
    }

    IFileProvider standardFileProvider = configurationBuilder.GetFileProvider();

    CompositeFileProvider compositeFileProvider = new(
      standardFileProvider,
      new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
    );

    configurationBuilder.SetFileProvider(compositeFileProvider);

    configurationBuilder.AddJsonFile($"{_serviceName}.config.json", optional: true, reloadOnChange: false);

    if (Environment.IsDevelopment())
    {
      configurationBuilder.AddJsonFile(
        $"{_serviceName}.config.development.json",
        optional: true,
        reloadOnChange: false
      );
    }

    return configurationBuilder;
  }

  [PublicAPI]
  protected virtual IMvcCoreBuilder PostConfigureMvc(IMvcCoreBuilder builder) => builder;

  [PublicAPI]
  protected virtual MvcOptions PreConfigureMvcOptions(MvcOptions options) => options;

  [PublicAPI]
  protected virtual MvcOptions PostConfigureMvcOptions(MvcOptions options) => options;

  [PublicAPI]
  protected virtual JsonOptions ConfigureJsonOptions(JsonOptions jsonOptions) => jsonOptions;

  [PublicAPI]
  protected virtual IServiceCollection ConfigureServices(IServiceCollection services) =>
    services.AddMessageWriter();

  [PublicAPI]
  protected virtual WebApplication PostAppBuild(WebApplication app) => app;

  [PublicAPI]
  protected virtual WebApplication PreAppRun(WebApplication app) => app;

  [PublicAPI]
  protected virtual WebApplication PostAppRun(WebApplication app, CancellationToken cancelToken = default) =>
    app;
}