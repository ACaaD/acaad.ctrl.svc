using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Transformations;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public class BaseTransformation : ITransformation
{
}