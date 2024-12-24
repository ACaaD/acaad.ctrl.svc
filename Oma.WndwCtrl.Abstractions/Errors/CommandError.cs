using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record CommandError : Error, ICommandExecutionMetadata
{
    private CommandError(TechnicalError technicalError)
    {
        Message = technicalError.Message;
        IsExceptional = technicalError.IsExceptional;
        IsExpected = technicalError.IsExpected;
        
        Inner = Option<Error>.Some(technicalError);
    }
    
    public override bool Is<E>()
    {
        // TODO: Wrong implementation
        if(this is E) return true;
        return false;
    }

    public override ErrorException ToErrorException()
    {
        throw new NotImplementedException();
    }

    public override string Message { get; }
    public override bool IsExceptional { get; }
    public override bool IsExpected { get; }

    public override Option<Error> Inner { get; } = Option<Error>.None;

    public Option<TimeSpan> ExecutionDuration { get; set; } = Option<TimeSpan>.None;
    public Option<int> ExecutedRetries { get; set; } = Option<int>.None;

    public static implicit operator CommandError(TechnicalError error)
        => new(error);
}