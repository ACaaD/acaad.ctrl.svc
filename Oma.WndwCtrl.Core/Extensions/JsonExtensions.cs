using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Extensions;

public static class JsonExtensions
{
    public static void AddNativePolymorphicTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        Type baseValueObjectType = typeof(ITransformation);
        
        if (jsonTypeInfo.Type == baseValueObjectType) {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions {
                TypeDiscriminatorPropertyName = "type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };
            var types = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ITransformation)));
            
            foreach (var t in types.Select(t => new JsonDerivedType(t, t.Name.Replace("Transformation", string.Empty).ToLowerInvariant()))) // Fixed ToLower() => ToLowerInvariant
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(t);
        }
    }
}