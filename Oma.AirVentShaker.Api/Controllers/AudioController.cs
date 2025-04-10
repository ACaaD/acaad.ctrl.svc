using Microsoft.AspNetCore.Mvc;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Controllers;

[ApiController]
[Route("vent/[controller]")]
public class AudioController(IAudioService audioService) : ControllerBase
{
  [HttpPost("test")]
  public async Task<ActionResult> GenerateTestWaveAsync(
    [FromBody] IWaveDescriptor waveDescriptor,
    [FromQuery(Name = "duration")] TimeSpan duration
  )
  {
    await audioService.PlayAsync(waveDescriptor, duration, HttpContext.RequestAborted);
    return Ok();
  }
}