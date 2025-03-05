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
    public async Task<IActionResult> GetMonitoringProjectListAsync(GetMonitoringProjectListRequest request)
    {
        var response = await Mediator.RequestAsync<GetMonitoringProjectListRequest, BaseResponse<GetMonitoringProjectListResponse>>(request);
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
    
    /// <summary>
    ///  Modify project api
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPatch("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ModifyMonitoringProjectApiAsync(ModifyMonitoringProjectApiCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Get project api request header list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/requestHeads")]
    [ProducesResponseType<BaseResponse<GetMonitoringProjectApiRequestHeaderListResponse>>(200)]
    public async Task<IActionResult> GetMonitoringProjectApiRequestHeaderListAsync(GetMonitoringProjectApiRequestHeaderListRequest request)
    {
        var response =
            await Mediator
                .RequestAsync<GetMonitoringProjectApiRequestHeaderListRequest,
                    BaseResponse<GetMonitoringProjectApiRequestHeaderListResponse>>(request);
        return Ok(response);
    }
    
    /// <summary>
    ///  Modify project api request header
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPatch("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/requestHeads/{requestHeaderInfoId:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ModifyMonitoringProjectApiRequestHeaderAsync(ModifyMonitoringProjectApiRequestHeaderCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Add project api request header
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/requestHeads/")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddMonitoringProjectApiRequestHeaderAsync(AddMonitoringProjectApiRequestHeaderCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Get project api query parameter list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/queryParameters")]
    [ProducesResponseType<BaseResponse<GetMonitoringProjectApiQueryParameterListResponse>>(200)]
    public async Task<IActionResult> GetMonitoringProjectApiQueryParameterListAsync(GetMonitoringProjectApiQueryParameterListRequest request)
    {
        var response =
            await Mediator
                .RequestAsync<GetMonitoringProjectApiQueryParameterListRequest,
                    BaseResponse<GetMonitoringProjectApiQueryParameterListResponse>>(request);
        return Ok(response);
    }
    
    /// <summary>
    ///  Modify project api query parameter
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPatch("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/queryParameters/{queryParameterId}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ModifyMonitoringProjectApiQueryParameterAsync(ModifyMonitoringProjectApiQueryParameterCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Add project api query parameter
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/queryParameters")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddMonitoringProjectApiQueryParameterAsync(AddMonitoringProjectApiQueryParameterCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
    
    /// <summary>
    ///  Delete project api query parameter
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpDelete("{monitoringProjectId:guid}/monitoringProjectApis/{monitoringProjectApiId:guid}/queryParameters/{queryParameterId}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> DeleteMonitoringProjectApiQueryParameterAsync(DeleteMonitoringProjectApiQueryParameterCommand command)
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
}