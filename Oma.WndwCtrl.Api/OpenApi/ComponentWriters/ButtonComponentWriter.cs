using LanguageExt;
using Oma.WndwCtrl.Api.OpenApi.Interfaces;
using Oma.WndwCtrl.Api.OpenApi.Model;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.OpenApi.ComponentWriters;

public class ButtonComponentWriter(ILogger<ButtonComponentWriter> logger) : IOpenApiComponentWriter<Button>
{
  public Task<Option<OpenApiComponentExtension>> CreateExtensionAsync(Button component) =>
    Task.FromResult(
      Option<OpenApiComponentExtension>.Some(
        new OpenApiComponentExtension(component)
      )
    );

  public ILogger Logger { get; } = logger;
}