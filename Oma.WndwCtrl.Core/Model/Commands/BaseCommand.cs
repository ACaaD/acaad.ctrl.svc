using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Interfaces;

namespace Oma.WndwCtrl.Core.Model.Commands;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CliCommand), typeDiscriminator: "cli")]
public class BaseCommand : ICommand
{
    public int Retries { get; set; } = 3;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}
