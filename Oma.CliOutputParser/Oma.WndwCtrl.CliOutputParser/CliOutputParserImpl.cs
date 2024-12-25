using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.CliOutputParser.Errors;
using Oma.WndwCtrl.CliOutputParser.Grammar;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.CliOutputParser.Visitors;

using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.CliOutputParser;

public class CliOutputParserImpl(IParserLogger parserLogger) : ICliOutputParser
{
    public Either<Error, IEnumerable<object>> Parse(string transformation, string text)
    {
        CollectingErrorListener errorListener = new();
        AntlrInputStream charStream = new(transformation);
        CliOutputLexer lexer = new(charStream);

        lexer.AddErrorListener(errorListener);

        CommonTokenStream tokenStream = new(lexer);

        CliOutputParser.Grammar.CliOutputParser parser = new(tokenStream);
        parser.AddErrorListener(errorListener);

        CliOutputParser.Grammar.CliOutputParser.TransformationContext? tree = parser.transformation();

        if (errorListener.Errors.Count > 0)
        {
            return Left(Error.Many(errorListener.Errors.Cast<Error>().ToArray()));
        }

        TransformationListener listener = new(parserLogger.Log, text);

        ParseTreeWalker walker = new();
        walker.Walk(listener, tree);

        return Right(listener.CurrentValues);
    }
}
