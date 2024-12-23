using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record CommandError : Error, ICommandExecutionMetadata
{
    private CommandError(TechnicalError technicalError)
    {
        Message = technicalError.Message;
        IsExceptional = technicalError.IsExceptional;
        IsExpected = technicalError.IsExpected;
    }
    
    public override bool Is<E>()
    {
        throw new NotImplementedException();
    }

    public override ErrorException ToErrorException()
    {
        throw new NotImplementedException();
    }

    public override string Message { get; }
    public override bool IsExceptional { get; }
    public override bool IsExpected { get; }
    
    public TimeSpan ExecutionDuration { get; set; }
    public int ExecutedRetries { get; set; }

    public static implicit operator CommandError(TechnicalError error)
        => new(error);
}