using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.Controllers.Components;

public class ButtonController : ComponentControllerBase<Switch>
{
  [HttpPost("trigger")]
  [EndpointSummary("Trigger")]
  public async Task<IActionResult> TriggerAsync()
  {
    await Task.Delay(0);
    throw new NotImplementedException();
  }
}