using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Traits;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

[SuppressMessage(
  "ReSharper",
  "UnusedMember.Global",
  Justification = "Won't fix: Additional functions may be required in the future."
)]
public class FlowT<TFlowConfiguration, A>(ReaderT<TFlowConfiguration, EitherT<FlowError, IO>, A> executeFlow)
  : K<Flow<TFlowConfiguration>, A>
{
  public ReaderT<TFlowConfiguration, EitherT<FlowError, IO>, A> ExecuteFlow { get; } = executeFlow;

  public FlowT<TFlowConfiguration, B> Map<B>(Func<A, B> f)
  {
    return this.Kind().Map(f).As();
  }

  public FlowT<TFlowConfiguration, B> Select<B>(Func<A, B> f)
  {
    return this.Kind().Map(f).As();
  }

  public FlowT<TFlowConfiguration, C> SelectMany<B, C>(
    Func<A, K<Flow<TFlowConfiguration>, B>> bind,
    Func<A, B, C> project
  )
  {
    return Bind(a => bind(a).Map(b => project(a, b)));
  }

  public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, IO<B>> bind, Func<A, B, C> project)
  {
    return SelectMany(a => MonadIO.liftIO<Flow<TFlowConfiguration>, B>(bind(a)), project);
  }

  public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, K<IO, B>> bind, Func<A, B, C> project)
  {
    return SelectMany(a => MonadIO.liftIO<Flow<TFlowConfiguration>, B>(bind(a).As()), project);
  }

  public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, Pure<B>> bind, Func<A, B, C> project)
  {
    return Map(a => project(a, bind(a).Value));
  }

  public static implicit operator FlowT<TFlowConfiguration, A>(Pure<A> ma)
  {
    return Flow<TFlowConfiguration>.Pure(ma.Value).As();
  }

  public static implicit operator FlowT<TFlowConfiguration, A>(IO<A> ma)
  {
    return Flow<TFlowConfiguration>.liftIO(ma);
  }

  public static implicit operator FlowT<TFlowConfiguration, A>(Either<FlowError, A> ma)
  {
    return Flow<TFlowConfiguration>.lift(ma);
  }

  [PublicAPI]
  public FlowT<TFlowConfiguration, B> Bind<B>(Func<A, K<Flow<TFlowConfiguration>, B>> f)
  {
    return this.Kind().Bind(f).As();
  }

  public static FlowT<TFlowConfiguration, A> operator >> (
    FlowT<TFlowConfiguration, A> ma,
    K<Flow<TFlowConfiguration>, A> mb
  )
  {
    return ma.Bind(_ => mb);
  }
  // These are from card game
}