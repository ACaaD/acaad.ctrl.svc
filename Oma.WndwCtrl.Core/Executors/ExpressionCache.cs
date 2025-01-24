using System.Linq.Expressions;
using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;

namespace Oma.WndwCtrl.Core.Executors;

[UsedImplicitly]
public class ExpressionCache : IExpressionCache
{
  private readonly Lock _commandMutex = new();
  private readonly Lock _transformationMutex = new();

  private Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>?
    _compiledCommandExecutorStack;

  private Func<TransformationConfiguration, EnvIO,
    ValueTask<Either<FlowError, TransformationOutcome>>>? _compiledTransformationStack;

  public Func<TransformationConfiguration, EnvIO,
    ValueTask<Either<FlowError, TransformationOutcome>>> GetOrCompile(
    Expression<Func<TransformationConfiguration, EnvIO,
      ValueTask<Either<FlowError, TransformationOutcome>>>> expression
  )
  {
    if (_compiledTransformationStack is not null)
    {
      return _compiledTransformationStack;
    }

    using (_transformationMutex.EnterScope())
    {
      if (_compiledTransformationStack is not null)
      {
        return _compiledTransformationStack;
      }

      _compiledTransformationStack = expression.Compile();
      return _compiledTransformationStack;
    }
  }

  public Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>> GetOrCompile(
    Expression<Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>> expression
  )
  {
    if (_compiledCommandExecutorStack is not null)
    {
      return _compiledCommandExecutorStack;
    }

    using (_commandMutex.EnterScope())
    {
      if (_compiledCommandExecutorStack is not null)
      {
        return _compiledCommandExecutorStack;
      }

      _compiledCommandExecutorStack = expression.Compile();
      return _compiledCommandExecutorStack;
    }
  }
}