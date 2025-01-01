using LanguageExt;
using LanguageExt.Traits;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

public class Flow<TFlowConfiguration> :
  Monad<Flow<TFlowConfiguration>>,
  Foldable<Flow<TFlowConfiguration>>
{
  /* Fold */
  /// <inheritdoc />
  public static S FoldWhile<A, S>(
    Func<A, Func<S, S>> f,
    Func<(S State, A Value), bool> predicate,
    S initialState,
    K<Flow<TFlowConfiguration>, A> ta
  )
  {
    throw new NotImplementedException();
  }

  public static S FoldBackWhile<A, S>(
    Func<S, Func<A, S>> f,
    Func<(S State, A Value), bool> predicate,
    S initialState,
    K<Flow<TFlowConfiguration>, A> ta
  )
  {
    throw new NotImplementedException();
  }

  public static K<Flow<TFlowConfiguration>, A> Pure<A>(A value)
  {
    FlowT<TFlowConfiguration, A> result = new(Prelude.Pure(value));
    return result;
  }

  /* MONAD */
  public static K<Flow<TFlowConfiguration>, B> Apply<A, B>(
    K<Flow<TFlowConfiguration>, Func<A, B>> mf,
    K<Flow<TFlowConfiguration>, A> ma
  )
  {
    return new FlowT<TFlowConfiguration, B>(mf.As().ExecuteFlow.Apply(ma.As().ExecuteFlow).As());
  }

  public static K<Flow<TFlowConfiguration>, B> Map<A, B>(Func<A, B> f, K<Flow<TFlowConfiguration>, A> ma)
  {
    return new FlowT<TFlowConfiguration, B>(ma.As().ExecuteFlow.Map(f));
  }

  public static K<Flow<TFlowConfiguration>, B> Bind<A, B>(
    K<Flow<TFlowConfiguration>, A> ma,
    Func<A, K<Flow<TFlowConfiguration>, B>> f
  )
  {
    return new FlowT<TFlowConfiguration, B>(ma.As().ExecuteFlow.Bind(a => f(a).As().ExecuteFlow));
  }

  public static K<Flow<TFlowConfiguration>, A> LiftIO<A>(IO<A> ma)
  {
    return new FlowT<TFlowConfiguration, A>(MonadIO
      .liftIO<ReaderT<TFlowConfiguration, EitherT<FlowError, IO>>, A>(ma).As());
  }

  /* Transformer Stack */
  public static FlowT<TFlowConfiguration, A> lift<A>(Either<FlowError, A> ma)
  {
    return new FlowT<TFlowConfiguration, A>(
      ReaderT.lift<TFlowConfiguration, EitherT<FlowError, IO>, A>(EitherT<FlowError, IO>.lift(ma)));
  }

  public static FlowT<TFlowConfiguration, A> liftIO<A>(IO<A> ma)
  {
    return new FlowT<TFlowConfiguration, A>(
      ReaderT.liftIO<TFlowConfiguration, EitherT<FlowError, IO>, A>(ma));
  }

  public static IO<A> liftAsync<A>(Func<EnvIO, Task<A>> f)
  {
    return IO<A>.LiftAsync(f);
  }

  /* Reader */
  public static FlowT<TFlowConfiguration, A> asks2<A>(Func<TFlowConfiguration, A> f)
  {
    return new FlowT<TFlowConfiguration, A>(ReaderT.asks<EitherT<FlowError, IO>, A, TFlowConfiguration>(f));
  }
}