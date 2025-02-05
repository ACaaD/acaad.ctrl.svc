using LanguageExt;
using Microsoft.OpenApi.Any;
using Oma.WndwCtrl.Api.OpenApi.Interfaces;
using Oma.WndwCtrl.Api.OpenApi.Model;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.OpenApi.ComponentWriters;

public class SwitchComponentWriter(ILogger<SwitchComponentWriter> logger) : IOpenApiComponentWriter<Switch>
{
  public Task<Option<OpenApiComponentExtension>> CreateExtensionAsync(Switch component)
  {
    OpenApiComponentExtension componentExtension = new(component)
    {
      ["onIff"] = MapOnIff(component),
    };

    return Task.FromResult(
      Option<OpenApiComponentExtension>.Some(
        componentExtension
      )
    );
  }

  public ILogger Logger { get; } = logger;

  private static IOpenApiPrimitive MapOnIff(Switch component)
    => component.OnIff switch
    {
      // TODO: Quite ugly to have different type mappings in Switch and here.. Oh well, fix later.
      string s => new OpenApiString(s),
      bool b => new OpenApiBoolean(b),
      long l => new OpenApiLong(l),
      decimal d => new OpenApiDouble((double)d),
      var _ => throw new ArgumentException(
        $"Unsupported type {component.OnIff.GetType()} for onIff (inferred value type: {component.InferredComparisonType})."
      ),
    };
}