using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Metrics;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public partial class DelegatingTransformer : IRootTransformer
{
  private static readonly FlowT<TransformationConfiguration, Unit> RecordTransformationDurationIO =
  (
    from c in Flow<TransformationConfiguration>.asks2(state => state.Command)
    from sa in Flow<TransformationConfiguration>.asks2(state => state.StartedAt)
    from m in Flow<TransformationConfiguration>.asks2(state => state.Metrics)
    from _ in Flow<TransformationConfiguration>.liftAsync(
      _ =>
      {
        m.RecordTransformationExecutionDuration(c, (DateTime.UtcNow - sa).TotalSeconds);
        return Task.FromResult(Unit.Default);
      }
    )
    select _
  ).As();

  private static readonly Expression<Func<TransformationConfiguration, EnvIO,
      ValueTask<Either<FlowError, TransformationOutcome>>>>
    _expression
      = (cfg, io) => OverallFlow.ExecuteFlow
        .Run(cfg)
        .Run()
        .RunAsync(io);


  private readonly ILogger<DelegatingTransformer> _logger;
  private readonly IAcaadCoreMetrics _metrics;
  private readonly IEnumerable<IOutcomeTransformer> _transformers;

  private readonly Func<TransformationConfiguration, EnvIO,
      ValueTask<Either<FlowError, TransformationOutcome>>>
    _transformerStack;

  public DelegatingTransformer(
    ILogger<DelegatingTransformer> logger,
    IEnumerable<IOutcomeTransformer> transformers,
    IAcaadCoreMetrics metrics,
    IExpressionCache expressionCache
  )
  {
    _logger = logger;
    _transformers = transformers;
    _metrics = metrics;

    Stopwatch swBuildStack = Stopwatch.StartNew();

    _transformerStack = expressionCache.GetOrCompile(_expression);

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
    from metrics in RecordTransformationDurationIO
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

    TransformationConfiguration initialConfiguration = new(_transformers, command, commandOutcome, _metrics);
    EnvIO envIO = EnvIO.New(token: cancelToken);

    Either<FlowError, TransformationOutcome> outcome =
      await _transformerStack.Invoke(initialConfiguration, envIO);

    LogFinishedTransformation(_logger, swExec.Measure(), outcome);

    return outcome;
  }

  [LoggerMessage(
    Level = LogLevel.Debug,
    Message =
      "Finished transformations in {elapsed} (Outcome={outcome})"
  )]
  public static partial void LogFinishedTransformation(
    ILogger logger,
    TimeSpan elapsed,
    Either<FlowError, TransformationOutcome> outcome
  );

  [ExcludeFromCodeCoverage]
  [PublicAPI]
  [SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Won't fix: Interface method"
  )]
  public bool Handles(ITransformation transformation) => true;

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