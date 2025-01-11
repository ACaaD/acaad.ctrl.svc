using LanguageExt;
using Oma.WndwCtrl.Api.OpenApi.Interfaces;
using Oma.WndwCtrl.Api.OpenApi.Model;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.OpenApi.ComponentWriters;

public class SwitchComponentWriter(ILogger<SwitchComponentWriter> logger) : IOpenApiComponentWriter<Switch>
{
  public Task<Option<OpenApiComponentExtension>> CreateExtensionAsync(Switch component) =>
    Task.FromResult(
      Option<OpenApiComponentExtension>.Some(
        new OpenApiComponentExtension(component)
      )
    );

  public ILogger Logger { get; } = logger;
}