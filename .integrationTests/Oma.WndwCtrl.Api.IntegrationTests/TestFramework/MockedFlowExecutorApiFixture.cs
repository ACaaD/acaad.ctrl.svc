using FluentAssertions;
using LanguageExt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework.Interfaces;
using Oma.WndwCtrl.Core.FlowExecutors;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.CoreAsp;

namespace Oma.WndwCtrl.Api.IntegrationTests.TestFramework;

public sealed class MockedFlowExecutorApiFixture : WebApplicationFactory<CtrlApiProgram>, IAsyncLifetime, IApiFixture
{
    public MockedFlowExecutorApiFixture()
    {
        WebApplicationWrapper<object>.ModifyJsonSerializerOptions(SystemTextJsonSerializerConfig.Options);
    }
    
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