using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Oma.WndwCtrl.CliOutputParser.Grammar;
using Oma.WndwCtrl.CliOutputParser.Visitors;

namespace Oma.WndwCtrl.CliOutputParser;

public record ProcessingError(string Message, int Line, int CharPositionInLine);

public record ProcessingError<TType>(string Message, int Line, int CharPositionInLine, TType OffendingSymbol) : ProcessingError(Message, Line, CharPositionInLine)
{
    public override string ToString() => $"[{Line}:{CharPositionInLine}] {Message} - {OffendingSymbol}";
}

public class CollectingErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    private readonly List<ProcessingError<int>> _lexerErrors = [];
    private readonly List<ProcessingError<IToken>> _parserErrors = [];

    public void SyntaxError(
        TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e
    )
    {
        _lexerErrors.Add(new(msg, line, charPositionInLine, offendingSymbol));
    }

    public void SyntaxError(
        TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e
    )
    {
        _parserErrors.Add(new(msg, line, charPositionInLine, offendingSymbol));
    }

    public List<ProcessingError> Errors => _lexerErrors.Cast<ProcessingError>().Concat(_parserErrors).ToList();
}

public class CliOutputParserImpl(Action<object> log)
{
    public IEnumerable<object> Parse(string transformation, string text)
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
            foreach (ProcessingError error in errorListener.Errors)
            {
                Console.WriteLine(error);
            }

            throw new InvalidOperationException(string.Join(Environment.NewLine, errorListener.Errors));
        }

        TransformationListener listener = new(log, text);

        ParseTreeWalker walker = new();
        walker.Walk(listener, tree);

        return listener.CurrentValues;
    }
}
