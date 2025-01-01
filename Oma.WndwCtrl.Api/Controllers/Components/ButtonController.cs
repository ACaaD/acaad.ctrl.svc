using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.Controllers.Components;

[SuppressMessage(
  "ReSharper",
  "RouteTemplates.MethodMissingRouteParameters",
  Justification = "Won't fix: Controller template; route parameters resolved through convention."
)]
public class ButtonController : ComponentControllerBase<Switch>
{
  [HttpPost("trigger")]
  [EndpointSummary("Trigger")]
  public async Task<IActionResult> TriggerAsync()
  {
    await Task.Delay(millisecondsDelay: 0);
    throw new NotImplementedException();
  }
}