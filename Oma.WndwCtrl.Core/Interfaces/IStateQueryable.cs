using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Interfaces;

[PublicAPI]
public interface IStateQueryable
{
  public ICommand QueryCommand { get; }
}