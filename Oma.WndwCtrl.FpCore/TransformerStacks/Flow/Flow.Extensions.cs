using LanguageExt;
using LanguageExt.Traits;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

public static class FlowExtensions
{
  public static FlowT<TFlowConfiguration, A> As<TFlowConfiguration, A>(this K<Flow<TFlowConfiguration>, A> ma)
  {
    return (FlowT<TFlowConfiguration, A>)ma;
  }

  public static FlowT<TFlowConfiguration, A> As2<TFlowConfiguration, A>(
    this K<Flow<TFlowConfiguration>, A> ma
  )
  {
    return (FlowT<TFlowConfiguration, A>)ma;
  }

  public static IO<Either<FlowError, A>> Run<TFlowConfiguration, A>(
    this K<Flow<TFlowConfiguration>, A> ma,
    TFlowConfiguration state
  )
  {
    return ma.As().ExecuteFlow.Run(state).As().Run().As();
  }

  public static FlowT<TFlowConfiguration, C> SelectMany<TFlowConfiguration, A, B, C>(
    this K<Flow<TFlowConfiguration>, A> ma,
    Func<A, K<Flow<TFlowConfiguration>, B>> bind,
    Func<A, B, C> project
  )
  {
    return ma.As().SelectMany(bind, project);
  }
}