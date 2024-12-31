using System.Collections;
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.CliOutputParser.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Interfaces;

public class ParserResult : List<object>
{
}

public interface ICliOutputParser
{
    Either<Error, ParserResult> Parse(string transformation, string text);
    
    Either<Error, ParserResult> Parse(string transformation, IEnumerable<object> values);
}