using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
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
  public async Task<IActionResult> QueryAsync()
  {
    await Task.Delay(millisecondsDelay: 0);
    return Ok(Component.QueryCommand);
  }

  [HttpPost("on")]
  [EndpointSummary("Switch ON")]
  public async Task<IActionResult> SwitchOnAsync()
  {
    await Task.Delay(millisecondsDelay: 0);
    throw new NotImplementedException();
  }

  [HttpPost("off")]
  [EndpointSummary("Switch OFF")]
  public async Task<IActionResult> SwitchOffAsync()
  {
    await Task.Delay(millisecondsDelay: 0);
    throw new NotImplementedException();
  }
}