using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.Model;
using Oma.WndwCtrl.Api.Transformations.CliParser;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.CoreAsp.Extensions;

namespace Oma.WndwCtrl.Api.Controllers;

[ApiController]
[Route(BaseRoute)]
public class TestController([FromKeyedServices(ServiceKeys.AdHocFlowExecutor)] IFlowExecutor flowExecutor)
  : ControllerBase
{
  public const string BaseRoute = "ctrl/test";
  public const string CommandRoute = "command";

  [FromServices] [UsedImplicitly]
  public required ICliOutputParser CliOutputParser { get; init; }

  [FromServices] [UsedImplicitly]
  public required ScopeLogDrain ParserLogDrain { get; init; }

  [HttpPost(CommandRoute)]
  [EndpointName($"Test_{nameof(TestCommandAsync)}")]
  [EndpointSummary("Test Command")]
  [EndpointDescription("Run an ad-hoc command")]
  [Produces("application/json")]
  public async Task<Either<FlowError, FlowOutcome>> TestCommandAsync([FromBody] ICommand command)
  {
    // TODO: Obvious security concerns here...
    // During development ok.
#if !DEBUG
            // return NotFound();
#endif

    Either<FlowError, FlowOutcome> flowResult =
      await flowExecutor.ExecuteAsync(command, HttpContext.RequestAborted);

    flowResult.RegisterForDispose(HttpContext);

    AppendLogsToHeader();

    return flowResult;
  }

  [HttpPost("transformation/parser")]
  [EndpointName($"Test_{nameof(TestTransformationCliParserAsync)}")]
  [EndpointSummary("Transformation - Cli Parser")]
  [EndpointDescription("Run an ad-hoc transformation processed by the CLI parser")]
  [Produces("application/json")]
  public IActionResult TestTransformationCliParserAsync([FromBody] TransformationTestRequest request)
  {
    Either<Error, ParserResult> transformResult = CliOutputParser.Parse(
      string.Join(Environment.NewLine, request.Transformation),
      string.Join(string.Empty, request.TestText)
    );

    AppendLogsToHeader();

    return transformResult.BiFold<IActionResult>(
      null!,
      Right: (_, outcome) => Ok(outcome),
      Left: (_, error) => Problem(
        error.Message,
        title: $"[{error.Code}] A {error.GetType().Name} occurred.",
        statusCode: error.IsExceptional
          ? 500
          : 400,
        extensions: error.Inner.IsSome
          ? new Dictionary<string, object?>
          {
            ["inner"] = new List<Error> { (Error)error.Inner, },
          }
          : error is ManyErrors manyErrors
            ? new Dictionary<string, object?>
            {
              ["inner"] = manyErrors.Errors,
            }
            : null
      )
    );
  }

  private void AppendLogsToHeader()
  {
    try
    {
      List<string> toAppend = ParserLogDrain.Messages.Select(
        m => m
          .Replace("\t", string.Empty)
          .Replace("\r", string.Empty)
          .Replace("\n", string.Empty)
      ).ToList();

      for (int i = 0; i < toAppend.Count; i++)
        HttpContext.Response.Headers.Append(
          $"cli-parser-logs-{i:d4}",
          toAppend[i]
        );
    }
    catch
    {
      // catch-all on purpose. Debugging feature.
    }
  }
}