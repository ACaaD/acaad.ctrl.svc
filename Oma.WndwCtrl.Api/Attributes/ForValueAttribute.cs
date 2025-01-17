using Microsoft.OpenApi.Any;

namespace Oma.WndwCtrl.Api.Attributes;

public class ForValueAttribute(bool value) : AcaadMetadataAttribute
{
  public override string Key => "forValue";

  public override IOpenApiPrimitive Value => new OpenApiBoolean(value);
}