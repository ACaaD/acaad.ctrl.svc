using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework.Interfaces;
using Oma.WndwCtrl.Core.Executors.Commands;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Api.IntegrationTests.TestFramework;

public sealed class MockedCommandExecutorApiFixture : WebApplicationFactory<CtrlApiProgram>, IAsyncLifetime,
  IApiFixture
{
  public MockedCommandExecutorApiFixture()
  {
    WebApplicationWrapper<IApiService>.ModifyJsonSerializerOptions(SystemTextJsonSerializerConfig.Options);
  }

  public ValueTask InitializeAsync() => ValueTask.CompletedTask;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    IMessageBus messageBusMock = Substitute.For<IMessageBus>();
    MessageBusAccessor accessorMock = Substitute.For<MessageBusAccessor>();
    accessorMock.MessageBus.Returns(messageBusMock);

    builder.ConfigureServices(
      services =>
      {
        // Remove potentially harmful executors, only allow dummy execution
        services.RemoveAll<ICommandExecutor>()
          .AddScoped<ICommandExecutor, DummyCommandExecutor>().AddSingleton(accessorMock);
      }
    );

    base.ConfigureWebHost(builder);
  }
}