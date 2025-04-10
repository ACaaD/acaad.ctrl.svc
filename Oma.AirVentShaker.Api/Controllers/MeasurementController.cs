using Microsoft.AspNetCore.Mvc;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Controllers;

[ApiController]
[Route("vent/[controller]")]
public class MeasurementController(ITestRunner testRunner) : ControllerBase
{
  [HttpPost("execute")]
  public async Task<ActionResult<TestSummary>> ExecuteTestAsync([FromBody] TestDefinition testDefinition) =>
    Ok(await testRunner.ExecuteAsync(testDefinition, HttpContext.RequestAborted));
}