using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.CliOutputParser.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Interfaces;

public interface ICliOutputParser
{
    Either<Error, IEnumerable<object>> Parse(string transformation, string text);
}