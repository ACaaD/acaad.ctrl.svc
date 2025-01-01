using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Extensions;

namespace Oma.WndwCtrl.Configuration.Model;

public class ComponentConfigurationAccessor
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
      .WithAddedModifier(JsonExtensions.GetPolymorphismModifierFor<ICommand>(
        t => t.Name.Replace("Command", string.Empty))
      )
      .WithAddedModifier(JsonExtensions.GetPolymorphismModifierFor<ITransformation>(
        t => t.Name.Replace("Transformation", string.Empty))
      ),
  };

  public ComponentConfiguration Configuration { get; set; } = new();

  public async static Task<ComponentConfigurationAccessor> FromFileAsync(
    CancellationToken cancelToken = default
  )
  {
    const string configurationFilePath = "component-configuration.json";

    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configurationFilePath);
    string fileContent = await File.ReadAllTextAsync(filePath, cancelToken);

    ComponentConfiguration? configuration =
      JsonSerializer.Deserialize<ComponentConfiguration>(fileContent, JsonOptions);

    if (configuration is null)
    {
      throw new InvalidOperationException("Could not load component configuration.");
    }

    return new ComponentConfigurationAccessor
    {
      Configuration = configuration,
    };
  }
}