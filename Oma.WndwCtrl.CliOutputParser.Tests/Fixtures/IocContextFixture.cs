using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.CliOutputParser.Extensions;
using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.CliOutputParser.Tests.Fixtures;

public sealed class IocContextFixture : IAsyncDisposable
{
  private readonly IServiceProvider _serviceProvider;

  public IocContextFixture()
  {
    ServiceCollection serviceCollection = [];
    serviceCollection.AddCliOutputParser();

    _serviceProvider = serviceCollection.BuildServiceProvider();
  }

  public ICliOutputParser Instance => _serviceProvider.GetRequiredService<ICliOutputParser>();

  public ValueTask DisposeAsync()
  {
    (_serviceProvider as IDisposable)?.Dispose();

    return ValueTask.CompletedTask;
  }
}