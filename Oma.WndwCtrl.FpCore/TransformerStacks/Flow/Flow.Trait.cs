using LanguageExt;
using LanguageExt.Traits;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

public class FlowT<TFlowConfiguration, A> : K<Flow<TFlowConfiguration>, A>
{
    public ReaderT<TFlowConfiguration, EitherT<FlowError, IO>, A> ExecuteFlow { get; }

    public FlowT(ReaderT<TFlowConfiguration, EitherT<FlowError, IO>, A> executeFlow)
    {
        ExecuteFlow = executeFlow;
    }
    
    public FlowT<TFlowConfiguration, B> Map<B>(Func<A, B> f) =>
        this.Kind().Map(f).As();
    
    public FlowT<TFlowConfiguration, B> Select<B>(Func<A, B> f) =>
        this.Kind().Map(f).As();
    
    public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, K<Flow<TFlowConfiguration>, B>> bind, Func<A, B, C> project) =>
        Bind(a => bind(a).Map(b => project(a, b)));
    
    public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, IO<B>> bind, Func<A, B, C> project) =>
        SelectMany(a => MonadIO.liftIO<Flow<TFlowConfiguration>, B>(bind(a)), project);
    
    public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, K<IO, B>> bind, Func<A, B, C> project) =>
        SelectMany(a => MonadIO.liftIO<Flow<TFlowConfiguration>, B>(bind(a).As()), project);

    public FlowT<TFlowConfiguration, C> SelectMany<B, C>(Func<A, Pure<B>> bind, Func<A, B, C> project) =>
        Map(a => project(a, bind(a).Value));
    
    public static implicit operator FlowT<TFlowConfiguration, A>(Pure<A> ma) =>
        Flow<TFlowConfiguration>.Pure(ma.Value).As();

    public static implicit operator FlowT<TFlowConfiguration, A>(IO<A> ma) =>
        Flow<TFlowConfiguration>.liftIO(ma);

    public static implicit operator FlowT<TFlowConfiguration, A>(Either<FlowError, A> ma) =>
        Flow<TFlowConfiguration>.lift(ma);
    
    // These are from card game
    public FlowT<TFlowConfiguration, B> Bind<B>(Func<A, K<Flow<TFlowConfiguration>, B>> f) =>
        this.Kind().Bind(f).As();
    
    public static FlowT<TFlowConfiguration, A> operator >>(FlowT<TFlowConfiguration, A> ma, K<Flow<TFlowConfiguration>, A> mb) =>
        ma.Bind(_ => mb);
    // These are from card game
}