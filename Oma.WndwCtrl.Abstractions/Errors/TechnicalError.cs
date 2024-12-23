using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record TechnicalError : Exceptional
{
    private const int ErrorCodeOffset = 500_000;
    
    public TechnicalError(string Message, int Code) : base(Message, ErrorCodeOffset + Code)
    {
    }
}