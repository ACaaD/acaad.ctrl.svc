using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Executors;
using Oma.WndwCtrl.Core.Executors.Commands;

namespace Oma.WndwCtrl.Core.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCommandExecutors(this IServiceCollection services)
    {
        services.AddScoped<ICommandExecutor, CliCommandExecutor>()
            .AddKeyedScoped<ICommandExecutor, DelegatingCommandExecutor>("entry-executor");
        
        return services;
    }
}