using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Extensions;

public static class JsonExtensions
{
  private static IEnumerable<Assembly> _assembliesToInspect = [];

  internal static void AddAssembliesToLoad(List<Assembly> assembliesToLoad)
  {
    if (assembliesToLoad.Count == 0)
    {
      return;
    }

    _assembliesToInspect = _assembliesToInspect.Union(assembliesToLoad).Distinct();
  }

  public static Action<JsonTypeInfo> GetPolymorphismModifierFor<T>(
    Func<Type, string> typeToDiscriminatorTransform
  )
  {
    List<Assembly> assembliesToSearch =
      [.._assembliesToInspect, typeof(BaseTransformation).Assembly, typeof(T).Assembly,];

    return jsonTypeInfo =>
    {
      Type baseType = typeof(T);

      if (jsonTypeInfo.Type != baseType)
      {
        return;
      }

      jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = "type",
        IgnoreUnrecognizedTypeDiscriminators = true,
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
      };

      IEnumerable<Type> types = assembliesToSearch.SelectMany(a => a.GetTypes())
        .Where(t => t is { IsClass: true, IsAbstract: false, } && t.IsAssignableTo(baseType));

      foreach (Type t in types)
      {
        JsonDerivedType derivedType = new(t, typeToDiscriminatorTransform(t).ToLowerInvariant());
        jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
      }
    };
  }
}