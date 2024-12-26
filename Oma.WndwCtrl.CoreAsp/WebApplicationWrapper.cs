using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.CoreAsp.Conventions;
using Scalar.AspNetCore;

namespace Oma.WndwCtrl.CoreAsp;

public class WebApplicationWrapper<TAssemblyDescriptor>
    where TAssemblyDescriptor: class
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
            .AddApiExplorer();

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

    protected virtual IConfigurationBuilder ConfigurationConfiguration(IConfigurationBuilder configurationBuilder) =>
        configurationBuilder;
    
    protected virtual IMvcCoreBuilder PostConfigureMvc(IMvcCoreBuilder builder) => builder;
    protected virtual MvcOptions PreConfigureMvcOptions(MvcOptions options) => options;
    protected virtual MvcOptions PostConfigureMvcOptions(MvcOptions options) => options;
    protected virtual IServiceCollection ConfigureServices(IServiceCollection services) => services;
    
    protected virtual WebApplication PostAppBuild(WebApplication app) => app;
    protected virtual WebApplication PreAppRun(WebApplication app) => app;

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