using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework.Interfaces;
using Oma.WndwCtrl.Core.Executors.Commands;
using Oma.WndwCtrl.CoreAsp;

namespace Oma.WndwCtrl.Api.IntegrationTests.TestFramework;

public sealed class MockedCommandExecutorApiFixture : WebApplicationFactory<CtrlApiProgram>, IAsyncLifetime,
  IApiFixture
{
  public MockedCommandExecutorApiFixture()
  {
    WebApplicationWrapper<object>.ModifyJsonSerializerOptions(SystemTextJsonSerializerConfig.Options);
  }

  public ValueTask InitializeAsync()
  {
    return ValueTask.CompletedTask;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
      // Remove potentially harmful executors, only allow dummy execution
      services.RemoveAll<ICommandExecutor>()
        .AddScoped<ICommandExecutor, DummyCommandExecutor>();
    });

    base.ConfigureWebHost(builder);
  }
}