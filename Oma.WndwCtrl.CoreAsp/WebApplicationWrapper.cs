using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Extensions;
using Oma.WndwCtrl.CoreAsp.Conventions;
using Scalar.AspNetCore;

namespace Oma.WndwCtrl.CoreAsp;

public class WebApplicationWrapper<TAssemblyDescriptor>
  where TAssemblyDescriptor : class
{
  protected WebApplication? Application { get; private set; }

  public async Task StartAsync(CancellationToken cancelToken = default, params string[] args)
  {
    if (Application is not null)
    {
      throw new InvalidOperationException("Application is already running.");
    }

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    ConfigurationConfiguration(builder.Configuration);

    IMvcCoreBuilder mvcBuilder = builder.Services
      .AddMvcCore(opts =>
      {
        PreConfigureMvcOptions(opts);

        opts.Conventions.Add(new ContainingAssemblyApplicationModelConvention<TAssemblyDescriptor>());

        PostConfigureMvcOptions(opts);
      })
      .AddApiExplorer()
      .AddJsonOptions(opts =>
      {
        ModifyJsonSerializerOptions(opts.JsonSerializerOptions);
        ConfigureJsonOptions(opts);
      });

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
      .WithAddedModifier(JsonExtensions.GetPolymorphismModifierFor<ICommand>(
        t => t.Name.Replace("Command", string.Empty))
      )
      .WithAddedModifier(JsonExtensions.GetPolymorphismModifierFor<ITransformation>(
        t => t.Name.Replace("Transformation", string.Empty))
      );
  }

  protected virtual IConfigurationBuilder ConfigurationConfiguration(
    IConfigurationBuilder configurationBuilder
  )
  {
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