using LanguageExt.Traits;

namespace Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

public static class FlowExtensions
{
    public static Flow<TFlowConfiguration, A> As<TFlowConfiguration, A>(this K<Flow<TFlowConfiguration>, A> ma) =>
        (Flow<TFlowConfiguration, A>)ma;
}