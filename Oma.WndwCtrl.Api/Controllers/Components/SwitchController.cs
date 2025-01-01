using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.Controllers.Components;

public class SwitchController : ComponentControllerBase<Switch>
{
  [HttpGet]
  [EndpointSummary("Query Switch")]
  public async Task<IActionResult> QueryAsync()
  {
    await Task.Delay(0);
    return Ok(Component.QueryCommand);
  }

  [HttpPost("on")]
  [EndpointSummary("Switch ON")]
  public async Task<IActionResult> SwitchOnAsync()
  {
    await Task.Delay(0);
    throw new NotImplementedException();
  }

  [HttpPost("off")]
  [EndpointSummary("Switch OFF")]
  public async Task<IActionResult> SwitchOffAsync()
  {
    await Task.Delay(0);
    throw new NotImplementedException();
  }
}