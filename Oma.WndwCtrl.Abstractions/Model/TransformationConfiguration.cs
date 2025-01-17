using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Metrics;

namespace Oma.WndwCtrl.Abstractions.Model;

public record TransformationConfiguration
{
  public TransformationConfiguration(
    IEnumerable<IOutcomeTransformer> outcomeTransformers,
    ICommand command,
    Either<FlowError, CommandOutcome> commandOutcome,
    IAcaadCoreMetrics metrics
  )
  {
    OutcomeTransformers = new Seq<IOutcomeTransformer>(outcomeTransformers);
    Command = command;
    Metrics = metrics;
    InitialOutcome = commandOutcome.Map(co => new TransformationOutcome(co));
  }

  public Seq<IOutcomeTransformer> OutcomeTransformers { get; }
  public ICommand Command { get; }
  public IAcaadCoreMetrics Metrics { get; }

  public Either<FlowError, TransformationOutcome> InitialOutcome { get; }
  public DateTime StartedAt { get; } = DateTime.UtcNow;
}