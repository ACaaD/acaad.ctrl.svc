using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Abstractions.Model;

public record TransformationState
{
    public ILogger Logger;
    public Seq<IOutcomeTransformer> OutcomeTransformers;
    public ICommand Command { get; set; }

    public TransformationState(
        ILogger logger,
        IEnumerable<IOutcomeTransformer> outcomeTransformers,
        ICommand command
    )
    {
        Logger = logger;
        OutcomeTransformers = new Seq<IOutcomeTransformer>(outcomeTransformers);
        Command = command;
    }
}