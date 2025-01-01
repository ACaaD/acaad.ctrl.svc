using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Abstractions.Model;

public record TransformationConfiguration
{
  public TransformationConfiguration(
    IEnumerable<IOutcomeTransformer> outcomeTransformers,
    ICommand command,
    Either<FlowError, CommandOutcome> commandOutcome
  )
  {
    OutcomeTransformers = new Seq<IOutcomeTransformer>(outcomeTransformers);
    Command = command;
    InitialOutcome = commandOutcome.Map(co => new TransformationOutcome(co));
  }

  public Seq<IOutcomeTransformer> OutcomeTransformers { get; }
  public ICommand Command { get; }

  public Either<FlowError, TransformationOutcome> InitialOutcome { get; }
}