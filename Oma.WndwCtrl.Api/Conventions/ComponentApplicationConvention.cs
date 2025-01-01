using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Oma.WndwCtrl.Api.Controllers.Components;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.Conventions;

public class ComponentApplicationConvention(ComponentConfigurationAccessor configurationAccessor)
  : IApplicationModelConvention
{
  private readonly ComponentConfigurationAccessor _configurationAccessor = configurationAccessor;

  [NotNull] private ControllerModel? _buttonBase = null;
  [NotNull] private ControllerModel? _sensorBase = null;
  [NotNull] private ControllerModel? _switchBase = null;

  public void Apply(ApplicationModel application)
  {
    PopulateControllerBases(application);

    foreach (KeyValuePair<string, Component> component in _configurationAccessor.Configuration.Components)
    {
      ControllerModel controllerModel = new(GetBaseController(component.Value))
      {
        ControllerName = component.Key,
      };

      UpdateSelectorRouteTemplates(controllerModel, component);
      AddComponentToActionProperties(controllerModel, component.Value);

      application.Controllers.Add(controllerModel);
    }

    ClearBaseControllers(application);
  }

  [MemberNotNull(nameof(_buttonBase))]
  [MemberNotNull(nameof(_sensorBase))]
  [MemberNotNull(nameof(_switchBase))]
  private void PopulateControllerBases(ApplicationModel application)
  {
    _buttonBase = application.Controllers.Single(c => c.ControllerType == typeof(ButtonController));
    _sensorBase = application.Controllers.Single(c => c.ControllerType == typeof(SensorController));
    _switchBase = application.Controllers.Single(c => c.ControllerType == typeof(SwitchController));
  }

  private ControllerModel GetBaseController(Component component)
  {
    return component switch
    {
      Button => _buttonBase,
      Sensor => _sensorBase,
      Switch => _switchBase,
      var _ => throw new InvalidOperationException(
        $"Component type {component.GetType().FullName} not supported."),
    };
  }

  private static void UpdateSelectorRouteTemplates(
    ControllerModel controllerModel,
    KeyValuePair<string, Component> component
  )
  {
    foreach (SelectorModel? selector in controllerModel.Selectors)
    {
      if (selector.AttributeRouteModel?.Template is null)
      {
        throw new InvalidOperationException("Template is not present on selector attribute.");
      }

      selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template
        .Replace("{componentName}", component.Key);
    }
  }

  private static void AddComponentToActionProperties(ControllerModel controllerModel, Component component)
  {
    foreach (ActionModel? action in controllerModel.Actions)
      action.Properties.Add(nameof(Component), component);
  }

  private void ClearBaseControllers(ApplicationModel application)
  {
    application.Controllers.Remove(_buttonBase!);
    application.Controllers.Remove(_sensorBase!);
    application.Controllers.Remove(_switchBase!);
  }
}