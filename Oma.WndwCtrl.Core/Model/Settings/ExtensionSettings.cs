using System.Reflection;

namespace Oma.WndwCtrl.Core.Model.Settings;

public class ExtensionSettings : List<AssemblyInfo2>
{
  public const string SectionName = "Extensions";

  public List<Assembly> GetAssemblies()
  {
    // TODO: Obvious security concerns..
    return this.Select(assemblyInfo => Assembly.LoadFrom($"{assemblyInfo.AssemblyName}")).ToList();
  }
}