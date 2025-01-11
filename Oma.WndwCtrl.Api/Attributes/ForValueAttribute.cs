using Microsoft.OpenApi.Any;

namespace Oma.WndwCtrl.Api.Attributes;

public class ForValueAttribute(bool value) : AcaadMetadataAttribute
{
  public override string Key => "for-value";

  public override IOpenApiPrimitive Value => new OpenApiBoolean(value);
}