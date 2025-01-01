using System.Diagnostics.Contracts;
using LanguageExt;
using LanguageExt.Common;

namespace Oma.WndwCtrl.Abstractions.Errors;

public record FlowError : Error
{
    protected FlowError(Error other) : this(other.Message, other.IsExceptional, other.IsExpected)
    {
        Code = other.Code;
        Inner = other;
    }

    protected FlowError(TechnicalError technicalError) : this((Error)technicalError)
    {
    }

    public FlowError(string message, bool isExceptional) : this(message, isExceptional, isExpected: !isExceptional)
    {
    }
    
    public FlowError(string message, bool isExceptional, bool isExpected)
    {
        Message = message;
        IsExceptional = isExceptional;
        IsExpected = isExpected;
    }

    public override ErrorException ToErrorException()
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [Pure]
    public static FlowError NoCommandExecutorFound(ICommand command) =>
        new FlowError(
            $"No transformation executor found that handles transformation type {command.GetType().FullName}.",
            isExceptional: false);

    [Pure]
    public static FlowError NoTransformerFound(ITransformation transformation) =>
        new FlowError(
            $"No transformation executor found that handles transformation type {transformation.GetType().FullName}.",
            isExceptional: false);

    public override int Code { get; }
    public override string Message { get; }
    public override bool IsExceptional { get; }
    public override bool IsExpected { get; }
    
    public override Option<Error> Inner { get; } = Option<Error>.None;
    
    public static implicit operator FlowError(TechnicalError error)
        => new(error);
}