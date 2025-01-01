using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Model;

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
  public IActionResult GetDetails()
  {
    return Ok(Component);
  }
}