using System.Linq.Expressions;
using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Core.Interfaces;

public interface IExpressionCache
{
  Func<TransformationConfiguration, EnvIO,
    ValueTask<Either<FlowError, TransformationOutcome>>> GetOrCompile(
    Expression<Func<TransformationConfiguration, EnvIO,
      ValueTask<Either<FlowError, TransformationOutcome>>>> expression
  );

  Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>> GetOrCompile(
    Expression<Func<CommandState, EnvIO, ValueTask<Either<FlowError, CommandOutcome>>>> expression
  );
}