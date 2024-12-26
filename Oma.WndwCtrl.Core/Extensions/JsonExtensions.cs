using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Extensions;

public static class JsonExtensions
{
    private static readonly Type _transformationType = typeof(ITransformation);
    private static readonly Assembly _assemblyToSearch = typeof(BaseTransformation).Assembly;
    
    public static void AddNativePolymorphicTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type != _transformationType)
        {
            return;
        }

        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "type",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        var types = _assemblyToSearch.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(ITransformation)));

        foreach (Type t in types)
        {
            JsonDerivedType derivedType = new(t, GetDiscriminatorForType(t));
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
        }
    }

    private static string GetDiscriminatorForType(Type t) 
        => t.Name.Replace("Transformation", string.Empty).ToLowerInvariant();
}