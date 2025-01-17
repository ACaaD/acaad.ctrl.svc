using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.OpenApi.Model;

namespace Oma.WndwCtrl.Api.OpenApi.Interfaces;

public interface IOpenApiComponentWriter
{
  ILogger Logger { get; }

  bool Handles(IComponent component);

  Task<Option<OpenApiComponentExtension>> CreateExtensionAsync(IComponent component);
}

public interface IOpenApiComponentWriter<in TComponent> : IOpenApiComponentWriter
  where TComponent : IComponent
{
  bool IOpenApiComponentWriter.Handles(IComponent component) => component is TComponent;

  async Task<Option<OpenApiComponentExtension>> IOpenApiComponentWriter.CreateExtensionAsync(
    IComponent component
  )
  {
    if (component is TComponent tComponent)
    {
      return await CreateExtensionAsync(tComponent);
    }

    Logger.LogWarning(
      "Expected component {actual} to be of type {name}. Skipping.",
      component.GetType().Name,
      typeof(TComponent).Name
    );

    return Option<OpenApiComponentExtension>.None;
  }

  Task<Option<OpenApiComponentExtension>> CreateExtensionAsync(TComponent component);
}