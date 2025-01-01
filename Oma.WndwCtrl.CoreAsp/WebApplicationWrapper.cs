using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Extensions;
using Oma.WndwCtrl.CoreAsp.Conventions;
using Scalar.AspNetCore;

namespace Oma.WndwCtrl.CoreAsp;

public class WebApplicationWrapper<TAssemblyDescriptor>
  where TAssemblyDescriptor : class
{
  private IWebHostEnvironment? _environment;
  protected WebApplication? Application { get; private set; }

  protected IWebHostEnvironment Environment
  {
    get => _environment ?? throw new InvalidOperationException($"{nameof(Environment)} is not populated.");
    private set => _environment = value;
  }

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

    ConfigurationConfiguration(builder.Configuration);

    IMvcCoreBuilder mvcBuilder = builder.Services
      .AddMvcCore(
        opts =>
        {
          PreConfigureMvcOptions(opts);

          opts.Conventions.Add(new ContainingAssemblyApplicationModelConvention<TAssemblyDescriptor>());

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

    ConfigureServices(builder.Services);

    Application = PostAppBuild(builder.Build());

    Application.MapControllers();
    Application.MapOpenApi();
    Application.MapScalarApiReference();

#if DEBUG
    Application.UseDeveloperExceptionPage();
#endif

    await PreAppRun(Application).StartAsync(cancelToken);
  }

  public static void ModifyJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
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
      );
  }

  protected virtual IConfigurationBuilder ConfigurationConfiguration(
    IConfigurationBuilder configurationBuilder
  )
  {
    string serviceName = typeof(TAssemblyDescriptor).Name;

    IFileProvider standardFileProvider = configurationBuilder.GetFileProvider();

    CompositeFileProvider compositeFileProvider = new(
      standardFileProvider,
      new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
    );

    configurationBuilder.SetFileProvider(compositeFileProvider);

    IList<IConfigurationSource> s = configurationBuilder.Sources;

    configurationBuilder.AddJsonFile($"{serviceName}.config.json", optional: true, reloadOnChange: false);

    if (Environment.IsDevelopment())
    {
      configurationBuilder.AddJsonFile(
        $"{serviceName}.config.development.json",
        optional: true,
        reloadOnChange: false
      );
    }

    return configurationBuilder;
  }

  protected virtual IMvcCoreBuilder PostConfigureMvc(IMvcCoreBuilder builder)
  {
    return builder;
  }

  protected virtual MvcOptions PreConfigureMvcOptions(MvcOptions options)
  {
    return options;
  }

  protected virtual MvcOptions PostConfigureMvcOptions(MvcOptions options)
  {
    return options;
  }

  protected virtual JsonOptions ConfigureJsonOptions(JsonOptions jsonOptions)
  {
    return jsonOptions;
  }

  protected virtual IServiceCollection ConfigureServices(IServiceCollection services)
  {
    return services;
  }

  protected virtual WebApplication PostAppBuild(WebApplication app)
  {
    return app;
  }

  protected virtual WebApplication PreAppRun(WebApplication app)
  {
    return app;
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
}