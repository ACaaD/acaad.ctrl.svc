using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
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
  public async Task<Either<FlowError, FlowOutcome>> QueryAsync()
    => await ExecuteCommandAsync(Component.QueryCommand);

  [HttpPost("on")]
  [EndpointSummary("Switch ON")]
  [Actionable]
  [ForValue(value: true)]
  public async Task<Either<FlowError, FlowOutcome>> SwitchOnAsync()
    => await ExecuteCommandAsync(Component.OnCommand);

  [HttpPost("off")]
  [EndpointSummary("Switch OFF")]
  [Actionable]
  [ForValue(value: false)]
  public async Task<Either<FlowError, FlowOutcome>> SwitchOffAsync()
    => await ExecuteCommandAsync(Component.OffCommand);
}