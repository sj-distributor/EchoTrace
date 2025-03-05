using EchoTrace.Controllers.Bases;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.MonitoringProjects;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers;

public class MonitoringProjectController : WebBaseController
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddMonitoringProjectAsync(AddMonitoringProjectCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    [HttpGet]
    [ProducesResponseType<BaseResponse<GetMonitoringProjectListResponse>>(200)]
    public async Task<IActionResult> GetMonitoringProjectListAsync(GetMonitoringProjectListRequest request)
    {
        var response = await Mediator.RequestAsync<GetMonitoringProjectListRequest, BaseResponse<GetMonitoringProjectListResponse>>(request);
        return Ok(response);
    }
}