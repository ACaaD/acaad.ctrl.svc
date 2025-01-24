using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Oma.WndwCtrl.CliOutputParser.Extensions;
using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.CliOutputParser.Tests.Fixtures;

public class IocContextFixture : IAsyncDisposable
{
  private ServiceCollection _serviceCollection;
  private IServiceProvider _serviceProvider;

  public IocContextFixture()
  {
    IConfiguration configMock = Substitute.For<IConfiguration>();

    _serviceCollection = [];

    _serviceCollection.AddCliOutputParser(configMock);

    _serviceProvider = _serviceCollection.BuildServiceProvider();
  }

  public ICliOutputParser Instance => _serviceProvider.GetRequiredService<ICliOutputParser>();

  public ValueTask DisposeAsync()
  {
    (_serviceProvider as IDisposable)?.Dispose();

    return ValueTask.CompletedTask;
  }
}