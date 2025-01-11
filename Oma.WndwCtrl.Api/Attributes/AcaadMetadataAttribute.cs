using Microsoft.OpenApi.Any;

namespace Oma.WndwCtrl.Api.Attributes;

public abstract class AcaadMetadataAttribute : AcaadAttribute
{
  public abstract string Key { get; }

  public virtual IOpenApiPrimitive Value { get; } = new OpenApiBoolean(value: true);
}