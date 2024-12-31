using LanguageExt;
using LanguageExt.Traits;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

public partial class Flow<TFlowConfiguration> : 
    Monad<Flow<TFlowConfiguration>>
{
    public static K<Flow<TFlowConfiguration>, A> Pure<A>(A value)
    {
        Flow<TFlowConfiguration, A> result = new Flow<TFlowConfiguration, A>(Prelude.Pure(value));
        return result;
    }
    
    /* MONAD */
    public static K<Flow<TFlowConfiguration>, B> Apply<A, B>(K<Flow<TFlowConfiguration>, Func<A, B>> mf, K<Flow<TFlowConfiguration>, A> ma) => 
        new Flow<TFlowConfiguration, B>(mf.As().ExecuteFlow.Apply(ma.As().ExecuteFlow).As());
    
    public static K<Flow<TFlowConfiguration>, B> Map<A, B>(Func<A, B> f, K<Flow<TFlowConfiguration>, A> ma) => 
        new Flow<TFlowConfiguration, B>(ma.As().ExecuteFlow.Map(f));
    
    public static K<Flow<TFlowConfiguration>, B> Bind<A, B>(K<Flow<TFlowConfiguration>, A> ma, Func<A, K<Flow<TFlowConfiguration>, B>> f) =>
        new Flow<TFlowConfiguration, B>(ma.As().ExecuteFlow.Bind(a => f(a).As().ExecuteFlow));
    
    /* Transformer Stack */
    public static Flow<TFlowConfiguration, A> lift<A>(Either<FlowError, A> ma) => 
        new (ReaderT.lift<TFlowConfiguration, EitherT<FlowError, IO>, A>(EitherT<FlowError, IO>.lift(ma)));
    
    public static Flow<TFlowConfiguration, A> liftIO<A>(IO<A> ma) => 
        new (ReaderT.liftIO<TFlowConfiguration, EitherT<FlowError, IO>, A>(ma));
}