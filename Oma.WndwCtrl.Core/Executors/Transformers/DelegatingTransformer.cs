using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Executors.Commands;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class DelegatingTransformer : IRootTransformer
{
    private readonly ILogger<DelegatingTransformer> _logger;
    private readonly IEnumerable<IOutcomeTransformer> _transformers;

    private readonly MyState<TransformationState, TransformationOutcome> _callChain;
    
    [ExcludeFromCodeCoverage]
    public bool Handles(ITransformation transformation) => true;

    public DelegatingTransformer(ILogger<DelegatingTransformer> logger, IEnumerable<IOutcomeTransformer> transformers)
    {
        _logger = logger;
        _transformers = transformers;
    }
    
    public async Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        ICommand command,
        Either<FlowError, CommandOutcome> commandOutcome, 
        CancellationToken cancelToken = default
    )
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(commandOutcome);
        _logger.LogTrace("Received command outcome to transform.");

        TransformationState initialState = new();
        
        var outcomeWithState = await _callChain.RunAsync(initialState);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);
        
        return outcomeWithState.BiBind<TransformationOutcome>( 
            tuple => tuple.Outcome, 
            err => err
        );
    }
}