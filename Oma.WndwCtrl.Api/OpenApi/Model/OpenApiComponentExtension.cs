using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Attributes;

namespace Oma.WndwCtrl.Api.OpenApi.Model;

public class OpenApiComponentExtension : OpenApiObject, IOpenApiExtension
{
  public OpenApiComponentExtension(IComponent component)
  {
    OpenApiObject componentExt = new();
    componentExt["name"] = new OpenApiString(component.Name);
    componentExt["type"] = new OpenApiString(component.Type);

    this["component"] = componentExt;
  }

  public OpenApiComponentExtension ApplyMetadata(List<AcaadAttribute> metadata)
  {
    foreach (AcaadMetadataAttribute attribute in metadata.OfType<AcaadMetadataAttribute>())
      this[attribute.Key] = attribute.Value;

    return this;
  }
}