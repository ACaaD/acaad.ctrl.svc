using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Api.Attributes;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.Controllers.Components;

[SuppressMessage(
  "ReSharper",
  "RouteTemplates.MethodMissingRouteParameters",
  Justification = "Won't fix: Controller template; route parameters resolved through convention."
)]
public class SwitchController : ComponentControllerBase<Switch>
{
  [HttpGet]
  [EndpointSummary("Query Switch")]
  [Queryable]
  public async Task<IActionResult> QueryAsync()
    => await ExecuteCommandAsync(Component.QueryCommand);

  [HttpPost("on")]
  [EndpointSummary("Switch ON")]
  [Actionable]
  [ForValue(value: true)]
  public async Task<IActionResult> SwitchOnAsync()
    => await ExecuteCommandAsync(Component.OnCommand);

  [HttpPost("off")]
  [EndpointSummary("Switch OFF")]
  [Actionable]
  [ForValue(value: false)]
  public async Task<IActionResult> SwitchOffAsync()
    => await ExecuteCommandAsync(Component.OffCommand);
}