using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Core.FlowExecutors;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.IntegrationTests.TestFramework;

public sealed class ApiAssemblyFixture : WebApplicationFactory<CtrlApiProgram>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddKeyedScoped<IFlowExecutor, NoOpFlowExecutor>(ServiceKeys.AdHocFlowExecutor);
        });
        base.ConfigureWebHost(builder);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;
}