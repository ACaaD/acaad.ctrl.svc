using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Metrics;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public class DelegatingCommandExecutor : ICommandExecutor
{
  private static readonly FlowT<CommandState, Unit> RecordExecutionDurationIO =
  (
    from c in Flow<CommandState>.asks2(state => state.Command)
    from sa in Flow<CommandState>.asks2(state => state.StartedAt)
    from m in Flow<CommandState>.asks2(state => state.Metrics)
    from _ in Flow<CommandState>.liftAsync(
      _ =>
      {
        m.RecordCommandExecutionDuration(c, (DateTime.UtcNow - sa).TotalSeconds);
        return Task.FromResult(Unit.Default);
      }
    )
    select _
  ).As();

  private static readonly FlowT<CommandState, Unit> WaitOnCompleteIO =
  (
    from c in Flow<CommandState>.asks2(state => state.Command)
    from _ in Flow<CommandState>.liftAsync(
      async envIO =>
      {
        await Task.Delay(c.WaitOnComplete, envIO.Token);
        return Unit.Default;
      }
    )
    select _
  ).As();

  private readonly IEnumerable<ICommandExecutor> _commandExecutors;
  private readonly ILogger<DelegatingCommandExecutor> _logger;
  private readonly IAcaadCoreMetrics _metrics;

  private readonly Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>
    _transformerStack;

  public DelegatingCommandExecutor(
    ILogger<DelegatingCommandExecutor> logger,
    IEnumerable<ICommandExecutor> commandExecutors,
    IAcaadCoreMetrics metrics
  )
  {
    _logger = logger;
    _commandExecutors = commandExecutors;
    _metrics = metrics;

    Stopwatch swBuildStack = Stopwatch.StartNew();

    Expression<Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>> expression
      = (cfg, io) => OverallFlow.ExecuteFlow
        .Run(cfg)
        .Run()
        .RunAsync(io);

    _transformerStack = expression.Compile();

    _logger.LogTrace("Build transformer stack in {elapsed}.", swBuildStack.Measure());
  }

  private static FlowT<CommandState, CommandOutcome> OverallFlow => (
    from cmd in Command
    from executor in FindApplicableExecutor
    from outcome in ExecuteExecutorIO(cmd, executor)
    from metric in RecordExecutionDurationIO
    from dly in WaitOnCompleteIO
    select outcome
  ).As();

  private static FlowT<CommandState, CommandState> Config =>
    new(ReaderT.ask<EitherT<FlowError, IO>, CommandState>());

  private static FlowT<CommandState, ICommand> Command =>
    Config.Map(cfg => cfg.Command)
      .As();

  private static FlowT<CommandState, Seq<ICommandExecutor>> Executors =>
    Config.Map(cfg => cfg.CommandExecutors)
      .As();

  private static FlowT<CommandState, ICommandExecutor> FindApplicableExecutor => (
    from executors in Executors
    from cmd in Command
    from found in Flow<CommandState>.lift(
      executors.Find(e => e.Handles(cmd))
        .ToEither<FlowError>(() => FlowError.NoCommandExecutorFound(cmd))
    )
    select found
  ).As();

  [ExcludeFromCodeCoverage]
  public bool Handles(ICommand command) => true;

  public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    ICommand cmd,
    CancellationToken cancelToken = default
  )
  {
    Stopwatch swExec = Stopwatch.StartNew();

    using IDisposable? ls = _logger.BeginScope(cmd);
    _logger.LogTrace("Received command to execute.");

    CommandState initialState = new(_commandExecutors, cmd, _metrics);
    EnvIO envIO = EnvIO.New(token: cancelToken);

    Either<FlowError, CommandOutcome> outcome = await _transformerStack.Invoke(initialState, envIO);

    _logger.LogDebug(
      "Finished command in {elapsed} (Success={isSuccess})",
      swExec.Measure(),
      outcome.IsRight
    );

    return outcome;
  }

  private static FlowT<CommandState, CommandOutcome> ExecuteExecutorIO(
    ICommand cmd,
    ICommandExecutor executor
  )
  {
    return (
      from _ in Flow<CommandState>.asks2(state => state.Command)
      from ioRes in Flow<CommandState>.liftAsync(
        async envIO =>
          await executor.ExecuteAsync(cmd, envIO.Token)
      )
      from result in Flow<CommandState>.lift(ioRes)
      select result
    ).As();
  }
}