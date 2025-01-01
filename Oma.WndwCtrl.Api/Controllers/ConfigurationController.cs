using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Configuration.Model;

namespace Oma.WndwCtrl.Api.Controllers;

[ApiController]
[Route("ctrl/[controller]")]
public class ConfigurationController(ComponentConfigurationAccessor configurationAccessor) : ControllerBase
{
  [HttpGet]
  [EndpointName($"Configuration_{nameof(GetConfiguration)}")]
  [EndpointSummary("Component Configuration")]
  [EndpointDescription("Returns the currently loaded (active) component configuration")]
  [Produces("application/json")]
  public IActionResult GetConfiguration()
  {
    return Ok(configurationAccessor.Configuration);
  }
}