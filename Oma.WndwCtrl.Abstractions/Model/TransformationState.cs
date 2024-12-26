using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Abstractions.Model;

public record TransformationState
{
    public ILogger Logger;
    public IEnumerable<IOutcomeTransformer> OutcomeTransformers;
    public ICommand Command { get; set; }
    public Either<FlowError, CommandOutcome> CommandExecutionOutcome { get; }
    public CancellationToken CancelToken { get; }

    public TransformationState(
        ILogger logger,
        IEnumerable<IOutcomeTransformer> outcomeTransformers,
        ICommand command,
        Either<FlowError, CommandOutcome> commandExecutionOutcome,
        CancellationToken cancelToken
    )
    {
        Logger = logger;
        OutcomeTransformers = outcomeTransformers;
        Command = command;
        CommandExecutionOutcome = commandExecutionOutcome;
        CancelToken = cancelToken;
    }

    public Option<DateTime> StartDate { get; init; }

    public ITransformation? CurrentTransformation { get; init; }
    
    public TransformationOutcome? CurrentOutcome { get; init; }
}