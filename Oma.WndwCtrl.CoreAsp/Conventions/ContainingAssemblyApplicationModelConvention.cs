using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Oma.WndwCtrl.CoreAsp.Conventions;

public class ContainingAssemblyApplicationModelConvention<TAssemblyDescriptor> : IApplicationModelConvention
  where TAssemblyDescriptor : class
{
  public void Apply(ApplicationModel application)
  {
    Assembly whitelistedAssembly = typeof(TAssemblyDescriptor).Assembly;

    foreach (ControllerModel controller in application.Controllers.ToList()
               .Where(controller => controller.ControllerType.Assembly != whitelistedAssembly))
      application.Controllers.Remove(controller);
  }
}