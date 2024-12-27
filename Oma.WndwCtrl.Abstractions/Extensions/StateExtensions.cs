using LanguageExt;
using Oma.WndwCtrl.Abstractions.Errors;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Abstractions.Extensions;

public delegate Task<Either<FlowError, (S State, A Outcome)>> MyState<S, A>(S state);

public static class StateExtensions
{
    public static MyState<S, B> BindAsync<S, A, B>(this MyState<S, A> ma, Func<A, MyState<S, B>> f) =>
        async state =>
        {
            try
            {
                var newIdk = await ma(state);
                return newIdk.Bind(pairA => f(pairA.Item2)(pairA.Item1).GetAwaiter().GetResult());
            }
            catch(Exception ex)
            {
                return Left<FlowError>(new TechnicalError(ex.Message, Code: 1337, ex));
            }
        };
    
    public async static Task<Either<FlowError, (S State, A Outcome)>> RunAsync<S, A>(this MyState<S, A> ma, S state)
    {
        try
        {
            return await ma(state);
        }
        catch (Exception ex)
        {
            return Left<FlowError>(new TechnicalError(ex.Message, Code: 1337, ex));
        }
    }
}