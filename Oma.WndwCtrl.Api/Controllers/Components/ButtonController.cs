using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.Controllers.Components;

[SuppressMessage(
  "ReSharper",
  "RouteTemplates.MethodMissingRouteParameters",
  Justification = "Won't fix: Controller template; route parameters resolved through convention."
)]
public class ButtonController : ComponentControllerBase<Button>
{
  [HttpPost("trigger")]
  [EndpointSummary("Trigger")]
  public async Task<IActionResult> TriggerAsync() => await ExecuteCommandAsync(Component.Command);
}