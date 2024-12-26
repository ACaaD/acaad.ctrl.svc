using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Model.Commands;

public class BaseCommand : ICommand
{
    public int Retries { get; set; }
    
    public TimeSpan Timeout { get; set; }
    
    public required IEnumerable<ITransformation> Transformations { get; set; }
}
