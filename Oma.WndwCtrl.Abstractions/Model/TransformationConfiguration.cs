using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Abstractions.Model;

public record TransformationConfiguration
{
    public ILogger Logger { get; }
    public Seq<IOutcomeTransformer> OutcomeTransformers { get; }
    public ICommand Command { get; }
    
    public Either<FlowError, TransformationOutcome> InitialOutcome { get; }

    public TransformationConfiguration(
        ILogger logger,
        IEnumerable<IOutcomeTransformer> outcomeTransformers,
        ICommand command,
        Either<FlowError, CommandOutcome> commandOutcome
    )
    {
        Logger = logger;
        OutcomeTransformers = new Seq<IOutcomeTransformer>(outcomeTransformers);
        Command = command;
        InitialOutcome = commandOutcome.Map(co => new TransformationOutcome(co));
    }
}