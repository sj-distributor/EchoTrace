using EchoTrace.Controllers.Bases;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.MonitoringProjects;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers;

public class MonitoringProjectController : WebBaseController
{
    /// <summary>
    ///  Create projects that need to be monitored
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddMonitoringProjectAsync(AddMonitoringProjectCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Get project list
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<BaseResponse<GetMonitoringProjectListResponse>>(200)]
    public async Task<IActionResult> GetMonitoringProjectListAsync()
    {
        var response = await Mediator.RequestAsync<GetMonitoringProjectListRequest, BaseResponse<GetMonitoringProjectListResponse>>(new GetMonitoringProjectListRequest());
        return Ok(response);
    }
    
    /// <summary>
    ///  Add project api
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("{monitoringProjectId:guid}/monitoringProjectApis")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddMonitoringProjectApiAsync(AddMonitoringProjectApiCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Get project api list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("{monitoringProjectId:guid}/monitoringProjectApis")]
    [ProducesResponseType<BaseResponse<GetMonitoringProjectApiListByMonitoringProjectIdResponse>>(200)]
    public async Task<IActionResult> GetMonitoringProjectApiListByMonitoringProjectIdAsync(GetMonitoringProjectApiListByMonitoringProjectIdRequest request)
    {
        var response = await Mediator
            .RequestAsync<GetMonitoringProjectApiListByMonitoringProjectIdRequest, BaseResponse<GetMonitoringProjectApiListByMonitoringProjectIdResponse>>(request);
        return Ok(response);
    }
}