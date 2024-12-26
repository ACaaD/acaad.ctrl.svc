using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Model.Commands;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CliCommand), typeDiscriminator: "cli")]
public class BaseCommand : ICommand
{
    [JsonConstructor]
    public BaseCommand(int retries, TimeSpan timeout, IList<BaseTransformation> transformations)
    {
        Retries = retries;
        Timeout = timeout;
        Transformations = transformations;
    }
    
    public int Retries { get; set; } = 3;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    
    public IEnumerable<ITransformation> Transformations { get; set; }
}
