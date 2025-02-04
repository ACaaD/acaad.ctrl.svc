using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Core.Errors.Transformations;

public record ParserTransformationError : TransformationError
{
  public ParserTransformationError(Error parserError) : base(parserError)
  {
  }
}