using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Configuration.Model;

namespace Oma.WndwCtrl.Api.Controllers;

[ApiController]
[Route("ctrl/[controller]")]
public class ConfigurationController : ControllerBase
{
  private readonly ComponentConfigurationAccessor _configurationAccessor;

  public ConfigurationController(ComponentConfigurationAccessor configurationAccessor)
  {
    _configurationAccessor = configurationAccessor;
  }

  [HttpGet]
  [EndpointName($"Configuration_{nameof(GetConfiguration)}")]
  [EndpointSummary("Component Configuration")]
  [EndpointDescription("Returns the currently loaded (active) component configuration")]
  [Produces("application/json")]
  public IActionResult GetConfiguration()
  {
    return Ok(_configurationAccessor.Configuration);
  }
}