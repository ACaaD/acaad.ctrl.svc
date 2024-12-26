using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Model.Commands;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CustomCommand), typeDiscriminator: "custom")]
[JsonDerivedType(typeof(BaseCommand), typeDiscriminator: "cli")]
public class BaseCommand : ICommand
{
    [JsonPropertyName("retries")]
    public int Retries { get; set; }

    [JsonPropertyName("timeout")]
    public TimeSpan Timeout { get; set; }

    [JsonPropertyName("transformations")]
    public IEnumerable<ITransformation> Transformations { get; set; }
}
