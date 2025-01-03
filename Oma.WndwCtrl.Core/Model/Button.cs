using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model;

/// <summary>
/// A write-only control that can be just executed, indicating success/failure of the operation
/// </summary>
public class Button : Component
{
  [JsonInclude]
  [JsonRequired]
  public ICommand Command { get; internal set; } = null!;
}