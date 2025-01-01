using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.CliOutputParser.Interfaces;

public class ParserResult : List<object>
{
}

public interface ICliOutputParser
{
  Either<Error, ParserResult> Parse(string transformation, string text);

  Either<Error, ParserResult> Parse(string transformation, IList<object> values);
}