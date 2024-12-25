using Antlr4.Runtime;
using Oma.WndwCtrl.CliOutputParser.Errors;

namespace Oma.WndwCtrl.CliOutputParser;

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