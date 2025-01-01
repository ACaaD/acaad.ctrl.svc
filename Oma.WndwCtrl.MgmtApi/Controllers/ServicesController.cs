using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.MgmtApi.Model;
using ServiceDescriptor = Oma.WndwCtrl.MgmtApi.Model.Api.ServiceDescriptor;

namespace Oma.WndwCtrl.MgmtApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ServicesController(ServiceState serviceState) : ControllerBase
{
  [HttpGet]
  [EndpointName($"Services_{nameof(GetAll)}")]
  [EndpointSummary("Service List")]
  [EndpointDescription("Lists all registered services")]
  [Produces("application/json")]
  public IActionResult GetAll()
  {
    return Ok(serviceState.All.Select(ServiceDescriptor.FromServiceWrapper));
  }

  [HttpGet("{serviceGuid:guid}")]
  [EndpointName($"Services_{nameof(GetByGuid)}")]
  [EndpointSummary("Service Details by GUID")]
  [EndpointDescription("Retrieves service details by GUID")]
  [Produces("application/json")]
  public IActionResult GetByGuid([FromRoute] Guid serviceGuid)
  {
    return GetDetail(sw => sw.ServiceGuid == serviceGuid);
  }

  [HttpGet("{serviceName}")]
  [EndpointName($"Services_{nameof(GetByName)}")]
  [EndpointSummary("Service Details by Name")]
  [EndpointDescription("Retrieves service details by Name")]
  [Produces("application/json")]
  public IActionResult GetByName([FromRoute] string serviceName)
  {
    return GetDetail(sw => sw.Name == serviceName);
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  public IActionResult GetDetail(Func<IServiceWrapper, bool> predicate)
  {
    IServiceWrapper? service = serviceState.All.FirstOrDefault(predicate);

    return service is null
      ? Problem("Could not locate service", statusCode: 404)
      : Ok(ServiceDescriptor.FromServiceWrapper(service));
  }


  [HttpPost("{serviceName}/start")]
  [EndpointSummary("Start Service by Name")]
  public Task<IActionResult> StartByNameAsync([FromRoute] string serviceName)
  {
    return StartAsync(sw => sw.Name == serviceName);
  }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
  [ApiExplorerSettings(IgnoreApi = true)]
  public async Task<IActionResult> StartAsync(Func<IServiceWrapper, bool> predicate)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
  {
    IServiceWrapper? service = serviceState.All.FirstOrDefault(predicate);

    if (service is null)
    {
      return Problem("Could not locate service to start", statusCode: 404);
    }

    await service.StartAsync(CancellationToken.None);

    return Ok();
  }

  [HttpPost("{serviceName}/stop")]
  [EndpointSummary("Stop Service by Name")]
  public Task<IActionResult> StopByNameAsync([FromRoute] string serviceName)
  {
    return StopAsync(sw => sw.Name == serviceName);
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  public async Task<IActionResult> StopAsync(Func<IServiceWrapper, bool> predicate)
  {
    IServiceWrapper? service = serviceState.All.FirstOrDefault(predicate);

    if (service is null)
    {
      return Problem("Could not locate service to stop", statusCode: 404);
    }

    if (service.Status is not ServiceStatus.Running)
    {
      return Problem($"Service {service.Name} is not running", statusCode: 400);
    }

    await service.StopAsync(HttpContext.RequestAborted);

    return Ok();
  }

  [HttpPost("restart")]
  [EndpointSummary("Restart all services")]
  public async Task<IActionResult> RestartAllAsync()
  {
    List<IServiceWrapper<IService>> all = serviceState.All.ToList();

    foreach (IServiceWrapper<IService>? service in all) await service.StopAsync();

    foreach (IServiceWrapper<IService>? service in all) await service.StartAsync();

    return Ok();
  }
}