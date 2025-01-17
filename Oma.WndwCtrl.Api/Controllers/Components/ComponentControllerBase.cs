using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.Attributes;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.CoreAsp.Extensions;

namespace Oma.WndwCtrl.Api.Controllers.Components;

[ApiController]
[Route("components/{componentName}")]
[SuppressMessage(
  "ReSharper",
  "RouteTemplates.MethodMissingRouteParameters",
  Justification = "Won't fix: Controller template; route parameters resolved through convention."
)]
[SuppressMessage(
  "ReSharper",
  "RouteTemplates.ControllerRouteParameterIsNotPassedToMethods",
  Justification = "Won't fix: Controller template; route parameters resolved through convention."
)]
public class ComponentControllerBase<TComponent> : ControllerBase
  where TComponent : Component
{
  [FromServices] [UsedImplicitly]
  public required IFlowExecutor FlowExecutor { get; init; }

  protected TComponent Component =>
    (ControllerContext.ActionDescriptor.Properties[nameof(Component)] as TComponent)!;

  [HttpGet("config")]
  [EndpointSummary("Component Details")]
  [AcaadHidden]
  public IActionResult GetDetails() => Ok(Component);

  [NonAction]
  protected async Task<IActionResult> ExecuteCommandAsync(ICommand command)
  {
    Either<FlowError, FlowOutcome> flowResult =
      await FlowExecutor.ExecuteAsync(command, HttpContext.RequestAborted);

    flowResult.RegisterForDispose(HttpContext);

    return flowResult.BiFold<IActionResult>(
      null!,
      Right: (_, outcome) => Ok(outcome),
      Left: (_, error) => Problem(
        error.Message,
        title: $"[{error.Code}] A {error.GetType().Name} occurred.",
        statusCode: error.IsExceptional
          ? 500
          : 400
      )
    );
  }
}