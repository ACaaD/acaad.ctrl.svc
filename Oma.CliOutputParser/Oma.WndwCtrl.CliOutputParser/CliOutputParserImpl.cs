using Antlr4.Runtime;
using Oma.WndwCtrl.CliOutputParser.Grammar;
using Oma.WndwCtrl.CliOutputParser.Visitors;

namespace Oma.WndwCtrl.CliOutputParser;

public record ProcessingError(string Message, int Line, int CharPositionInLine)
{
}

public record ProcessingError<TType>(string Message, int Line, int CharPositionInLine, TType OffendingSymbol) : ProcessingError(Message, Line, CharPositionInLine)
{
    public override string ToString() => $"[{Line}:{CharPositionInLine}] {Message} - {OffendingSymbol}";
}

public class CollectingErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    private List<ProcessingError<int>> _lexerErrors = new();
    private List<ProcessingError<IToken>> _parserErrors = new();
    
    public void SyntaxError(
        IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e
    )
    {
        _lexerErrors.Add(new(msg, line, charPositionInLine, offendingSymbol));
    }

    public void SyntaxError(
        IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e
    )
    {
        _parserErrors.Add(new(msg, line, charPositionInLine, offendingSymbol));
    }

    public List<ProcessingError> Errors => _lexerErrors.Cast<ProcessingError>().Concat(_parserErrors).ToList();
}

public class CliOutputParserImpl
{
    private readonly TransformationVisitor _transformationVisitor = new TransformationVisitor();
    
    public object? Parse(string transformation)
    {
        CollectingErrorListener errorListener = new();
        AntlrInputStream charStream = new(transformation);
        CliOutputLexer lexer = new(charStream);
        
        lexer.AddErrorListener(errorListener); 
        
        CommonTokenStream tokenStream = new(lexer);
        
        CliOutParser.Grammar.CliOutputParser parser = new(tokenStream);
        parser.AddErrorListener(errorListener);
        
        CliOutParser.Grammar.CliOutputParser.TransformationContext? tree = parser.transformation();
        
        if (errorListener.Errors.Count > 0)
        {
            foreach (var error in errorListener.Errors)
            {
                Console.WriteLine(error);
            }

            throw new InvalidOperationException(string.Join(Environment.NewLine, errorListener.Errors));
        }
        
        object? rules = tree.Accept(_transformationVisitor);
            
        return rules;
    }
}