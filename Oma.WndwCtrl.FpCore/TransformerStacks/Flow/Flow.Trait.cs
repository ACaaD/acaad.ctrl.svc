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