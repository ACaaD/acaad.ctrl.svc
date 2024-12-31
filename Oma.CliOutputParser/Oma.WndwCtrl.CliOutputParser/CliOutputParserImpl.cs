using System.Diagnostics.CodeAnalysis;
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
    public Either<Error, ParserResult> Parse(string transformation, string text)
    {
        TransformationListener Build()
            => new(parserLogger.Log, text);
        
        return Parse(transformation, Build);
    }
    
    public Either<Error, ParserResult> Parse(string transformation, IEnumerable<object> values)
    {
        TransformationListener Build()
            => new(parserLogger.Log, values);
        
        return Parse(transformation, Build);
    }
    
    private Either<Error, ParserResult> Parse(string transformation, Func<TransformationListener> transformationListenerFactory)
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
            return Error.Many(errorListener.Errors.Cast<Error>().ToArray());
        }

        TransformationListener listener = transformationListenerFactory();

        ParseTreeWalker walker = new();
        walker.Walk(listener, tree);

        var enumeratedList = listener.CurrentValues.ToList();

        if (enumeratedList.Count == 1)
        {
            return new ParserResult() { enumeratedList.Single() };
        }

        ParserResult result = new();

        foreach (var item in enumeratedList)
        {
            result.Add(item);
        }
        
        return result;
    }
}
