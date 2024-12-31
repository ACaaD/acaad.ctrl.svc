using LanguageExt;
using LanguageExt.Traits;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

public class Flow<TFlowConfiguration, A> : K<Flow<TFlowConfiguration>, A>
{
    public ReaderT<TFlowConfiguration, EitherT<FlowError, IO>, A> ExecuteFlow { get; }

    public Flow(ReaderT<TFlowConfiguration, EitherT<FlowError, IO>, A> executeFlow)
    {
        ExecuteFlow = executeFlow;
    }
    
    public static implicit operator Flow<TFlowConfiguration, A>(Pure<A> ma) =>
        Flow<TFlowConfiguration>.Pure(ma.Value).As();

    public static implicit operator Flow<TFlowConfiguration, A>(IO<A> ma) =>
        Flow<TFlowConfiguration>.liftIO(ma);

    public static implicit operator Flow<TFlowConfiguration, A>(Either<FlowError, A> ma) =>
        Flow<TFlowConfiguration>.lift(ma);
}