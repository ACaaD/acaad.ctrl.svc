using System.Text.Json;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.CoreAsp;

namespace Oma.WndwCtrl.Configuration.Model;

public class ComponentConfigurationAccessor
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  static ComponentConfigurationAccessor()
  {
    WebApplicationWrapper<IApiService>.ModifyJsonSerializerOptions(JsonOptions);
  }

  public ComponentConfiguration Configuration { get; set; } = new();

  public Option<Component> FindComponentByTrigger(ITrigger trigger)
  {
    foreach (Component component in Configuration.Components.Values)
      if (component.Triggers.Any(t => t.UniqueIdentifier == trigger.UniqueIdentifier))
      {
        return Option<Component>.Some(component);
      }

    return Option<Component>.None;
  }

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

    PostProcessConfiguration(configuration);

    return new ComponentConfigurationAccessor
    {
      Configuration = configuration,
    };
  }

  private static void PostProcessConfiguration(ComponentConfiguration componentConfiguration)
  {
    foreach (KeyValuePair<string, Component> component in componentConfiguration.Components)
      component.Value.Name = component.Key;
  }
}