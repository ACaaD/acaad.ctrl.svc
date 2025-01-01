using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Interfaces;

public interface IStateQueryable
{
  public ICommand QueryCommand { get; }
}