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
public class SensorController : ComponentControllerBase<Sensor>
{
  [HttpGet]
  [EndpointSummary("Query Sensor")]
  [Queryable]
  public async Task<IActionResult> QueryAsync() => await ExecuteCommandAsync(Component.QueryCommand);
}