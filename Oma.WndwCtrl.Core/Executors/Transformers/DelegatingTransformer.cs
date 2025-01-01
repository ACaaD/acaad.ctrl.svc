using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class DelegatingTransformer : IRootTransformer
{
  private readonly ILogger<DelegatingTransformer> _logger;
  private readonly IEnumerable<IOutcomeTransformer> _transformers;

  private readonly Func<TransformationConfiguration, EnvIO,
      ValueTask<Either<FlowError, TransformationOutcome>>>
    _transformerStack;

  public DelegatingTransformer(
    ILogger<DelegatingTransformer> logger,
    IEnumerable<IOutcomeTransformer> transformers
  )
  {
    _logger = logger;
    _transformers = transformers;

    Stopwatch swBuildStack = Stopwatch.StartNew();

    Expression<Func<TransformationConfiguration, EnvIO, ValueTask<Either<FlowError, TransformationOutcome>>>>
      expression
        = (cfg, io) => OverallFlow.ExecuteFlow
          .Run(cfg)
          .Run()
          .RunAsync(io);

    _transformerStack = expression.Compile();

    _logger.LogTrace("Build transformer stack in {elapsed}.", swBuildStack.Measure());
  }

  private static FlowT<TransformationConfiguration, TransformationOutcome> OverallFlow =>
  (
    from outcome in InitialOutcome
    from allT in Transformations

    // TODO: Presumably the order is incorrect here and the fold must be applied first, then the lift.
    // -> Write AUTs
    from eitherChained in
      allT.Fold<Seq, ITransformation, FlowT<TransformationConfiguration, TransformationOutcome>>(
        outcome,
        t =>
        {
          return lastEither =>
          {
            FlowT<TransformationConfiguration, TransformationOutcome> res =
              from transformer in FindApplicableTransformer(t)
              from result in ExecuteTransformerIO(t, transformer, lastEither)
              select result;

            return res;
          };
        }
      )
    select eitherChained
  ).As();

  private static FlowT<TransformationConfiguration, TransformationConfiguration> Config =>
    new(ReaderT.ask<EitherT<FlowError, IO>, TransformationConfiguration>());

  private static FlowT<TransformationConfiguration, Seq<ITransformation>> Transformations =>
    Config.Map(cfg => cfg.Command)
      .Map(command => command.Transformations)
      .Map(t => new Seq<ITransformation>(t))
      .As(); // TODO: Figure out why this is needed here.

  private static FlowT<TransformationConfiguration, Either<FlowError, TransformationOutcome>>
    InitialOutcome =>
    Config.Map(cfg => cfg.InitialOutcome).As();

  public async Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
    ICommand command,
    Either<FlowError, CommandOutcome> commandOutcome,
    CancellationToken cancelToken = default
  )
  {
    Stopwatch swExec = Stopwatch.StartNew();

    using IDisposable? ls = _logger.BeginScope(commandOutcome);
    _logger.LogTrace("Received command outcome to transform.");

    TransformationConfiguration initialConfiguration = new(_transformers, command, commandOutcome);
    EnvIO envIO = EnvIO.New(token: cancelToken);

    Either<FlowError, TransformationOutcome> outcome =
      await _transformerStack.Invoke(initialConfiguration, envIO);

    _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcome);

    return outcome;
  }

  [ExcludeFromCodeCoverage]
  [PublicAPI]
  [SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Won't fix: Interface method"
  )]
  public bool Handles(ITransformation transformation)
  {
    return true;
  }

  private static FlowT<TransformationConfiguration, IOutcomeTransformer> FindApplicableTransformer(
    ITransformation transformation
  )
  {
    return (
      from allTransformers in Config.Map(cfg => cfg.OutcomeTransformers)
      from found in Flow<TransformationConfiguration>.lift(
        allTransformers.Find(t => t.Handles(transformation))
          .ToEither<FlowError>(() => FlowError.NoTransformerFound(transformation))
      )
      select found
    ).As();
  }

  // TODO: Is passing the FlowT here correct?
  private static FlowT<TransformationConfiguration, TransformationOutcome> ExecuteTransformerIO(
    ITransformation transformation,
    IOutcomeTransformer transformer,
    FlowT<TransformationConfiguration, TransformationOutcome> outcome
  )
  {
    return (
      from unwrapped in outcome
      from ioRes in Flow<TransformationConfiguration>.liftAsync(
        async envIO =>
          await transformer.TransformCommandOutcomeAsync(transformation, unwrapped, envIO.Token)
      )
      from result in Flow<TransformationConfiguration>.lift(ioRes)
      select result
    ).As();
  }
}