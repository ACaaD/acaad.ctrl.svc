using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.Model;
using Oma.WndwCtrl.Api.Transformations.CliParser;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.FlowExecutors;
using Oma.WndwCtrl.Core.Model.Commands;

namespace Oma.WndwCtrl.Api.Controllers;

[ApiController]
[Route("ctrl/[controller]")]
public class TestController : ControllerBase
{
    [FromServices] public required AdHocFlowExecutor FlowExecutor { get; init; }
    
    [HttpPost("command")]
    [EndpointName($"Test_{nameof(TestCommandAsync)}")]
    [EndpointSummary("Test Command")]
    [EndpointDescription("Run an ad-hoc command")]
    [Produces("application/json")]
    public async Task<IActionResult> TestCommandAsync([FromBody] BaseCommand command) // TODO: Not that nice to require the base class instead of the interface here.
    {
        // TODO: Obvious security concerns here...
        #if !DEBUG
            return NotFound();
        #endif
        
        Either<FlowError, TransformationOutcome> flowResult =
            await FlowExecutor.ExecuteAsync(command, HttpContext.RequestAborted);

        return flowResult.BiFold<IActionResult>(
            state: null!,
            Right: (_, outcome) => Ok(outcome),
            Left: (_, error) => Problem(
                detail: error.Message, 
                title: $"[{error.Code}] A {error.GetType().Name} occurred.", 
                statusCode: error.IsExceptional ? 500 : 400,
                extensions: error.Inner.IsSome 
                    ? new Dictionary<string, object?>()
                    {
                        ["inner"] = (Error)error.Inner   
                    }
                    : null
            )
        );
    }
    
    [FromServices] public required ICliOutputParser CliOutputParser { get; init; }
    [FromServices] public required ScopeLogDrain ParserLogDrain { get; init; }

    [HttpPost("transformation/parser")]
    [EndpointName($"Test_{nameof(TestTransformationCliParserAsync)}")]
    [EndpointSummary("Transformation - Cli Parser")]
    [EndpointDescription("Run an ad-hoc transformation processed by the CLI parser")]
    [Produces("application/json")]
    public IActionResult TestTransformationCliParserAsync([FromBody]TransformationTestRequest request)
    {
        var transformResult = CliOutputParser.Parse(
            string.Join(string.Empty, request.Transformation),
            string.Join(string.Empty, request.TestText)
        );

        AppendLogsToHeader();
        
        return transformResult.BiFold<IActionResult>(
            state: null!,
            Right: (_, outcome) => Ok(outcome),
            Left: (_, error) => Problem(
                detail: error.Message, 
                title: $"[{error.Code}] A {error.GetType().Name} occurred.", 
                statusCode: error.IsExceptional ? 500 : 400,
                extensions: error.Inner.IsSome 
                    ? new Dictionary<string, object?>()
                    {
                        ["inner"] = new List<Error>() { (Error)error.Inner }   
                    }
                    : (error is ManyErrors manyErrors) ? new Dictionary<string, object?>()
                    {
                        ["inner"] = manyErrors.Errors  
                    } 
                    : null
            )
        );
    }

    private void AppendLogsToHeader()
    {
#if !DEBUG
        return;
#endif
        
        try
        {
            var toAppend = ParserLogDrain.Messages.Select(m => m
                .Replace("\t", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
            ).ToList();

            for (int i = 0; i < toAppend.Count; i++)
            {
                HttpContext.Response.Headers.Append(
                    $"cli-parser-logs-{i:d4}", 
                    toAppend[i]
                );   
            }
        }
        catch
        {
            // catch-all on purpose. Debugging feature.
        }
    }
}