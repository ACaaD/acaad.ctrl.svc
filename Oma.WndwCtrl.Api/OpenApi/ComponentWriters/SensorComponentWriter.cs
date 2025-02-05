using LanguageExt;
using Microsoft.OpenApi.Any;
using Oma.WndwCtrl.Api.OpenApi.Interfaces;
using Oma.WndwCtrl.Api.OpenApi.Model;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.OpenApi.ComponentWriters;

public class SensorComponentWriter(ILogger<SensorComponentWriter> logger) : IOpenApiComponentWriter<Sensor>
{
  public Task<Option<OpenApiComponentExtension>> CreateExtensionAsync(Sensor component)
  {
    OpenApiComponentExtension componentExtension = new(component)
    {
      ["type"] = new OpenApiString(component.QueryCommand.InferredType.ToString()),
      ["cardinality"] = new OpenApiString(component.QueryCommand.InferredCardinality.ToString()),
    };

    if (!string.IsNullOrEmpty(component.UnitOfMeasure))
    {
      componentExtension["unitOfMeasure"] = new OpenApiString(component.UnitOfMeasure);
    }

    return Task.FromResult(
      Option<OpenApiComponentExtension>.Some(
        componentExtension
      )
    );
  }

  public ILogger Logger { get; } = logger;
}