using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Transformations.CliParser;
using Oma.WndwCtrl.CliOutputParser;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Configuration.Extensions;
using Oma.WndwCtrl.Core.Extensions;

namespace Oma.WndwCtrl.Api.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddComponentApi(this IServiceCollection services)
    {
        services.AddConfiguration()
            .AddCommandExecutors()
            .TryAddSingleton<IApiService, CtrlApiService>();
            
        services
            // We actually want to override the parser to access it from the request scope
            .AddScoped<ICliOutputParser, CliOutputParserImpl>()  
            .AddScoped<ScopeLogDrain>()
            .AddScoped<IParserLogger>(sp => sp.GetRequiredService<ScopeLogDrain>());
        
        return services;
    }
}