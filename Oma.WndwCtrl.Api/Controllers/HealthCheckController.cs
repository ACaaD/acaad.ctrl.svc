using Microsoft.AspNetCore.Mvc;

namespace Oma.WndwCtrl.Api.Controllers;

[ApiController]
[Route("ctrl/[controller]")]
public class HealthCheckController : ControllerBase
{
  [HttpGet]
  [EndpointName($"HealthCheck_{nameof(Test)}")]
  [EndpointSummary("Health check")]
  [EndpointDescription("Dummy endpoint, always returns OK")]
  [Produces("application/json")]
  public IActionResult Test() => Ok();
}