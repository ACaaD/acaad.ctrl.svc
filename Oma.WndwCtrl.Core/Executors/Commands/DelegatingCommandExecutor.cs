using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public class DelegatingCommandExecutor : ICommandExecutor
{
  private readonly IEnumerable<ICommandExecutor> _commandExecutors;
  private readonly ILogger<DelegatingCommandExecutor> _logger;

  private readonly Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>
    _transformerStack;

  public DelegatingCommandExecutor(
    ILogger<DelegatingCommandExecutor> logger,
    IEnumerable<ICommandExecutor> commandExecutors
  )
  {
    _logger = logger;
    _commandExecutors = commandExecutors;

    Expression<Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>> expression
      = (cfg, io) => OverallFlow.ExecuteFlow
        .Run(cfg)
        .Run()
        .RunAsync(io);

    _transformerStack = expression.Compile();
  }

  private static FlowT<CommandState, CommandOutcome> OverallFlow => (
    from cmd in Command
    from executor in FindApplicableExecutor
    from outcome in ExecuteExecutorIO(cmd, executor)
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
  public bool Handles(ICommand command)
  {
    return true;
  }

  public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    ICommand cmd,
    CancellationToken cancelToken = default
  )
  {
    Stopwatch swExec = Stopwatch.StartNew();

    using IDisposable? ls = _logger.BeginScope(cmd);
    _logger.LogTrace("Received command to execute.");

    CommandState initialState = new(_logger, _commandExecutors, cmd);
    EnvIO envIO = EnvIO.New(token: cancelToken);

    Either<FlowError, CommandOutcome> outcome = await _transformerStack.Invoke(initialState, envIO);

    _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcome);

    return outcome;
  }

  private static FlowT<CommandState, CommandOutcome> ExecuteExecutorIO(
    ICommand cmd,
    ICommandExecutor executor
  )
  {
    return (
      from _ in Flow<CommandState>.asks2(state => state.Command)
      from ioRes in Flow<CommandState>.liftAsync(async envIO =>
        await executor.ExecuteAsync(cmd, envIO.Token)
      )
      from result in Flow<CommandState>.lift(ioRes)
      select result
    ).As();
  }
}