using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Model.Commands;
namespace Oma.WndwCtrl.Core.Interfaces;

public interface IStateQueryable
{
    public ICommand QueryCommand { get; }
}
