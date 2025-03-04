using EchoTrace.Controllers.Bases;
using EchoTrace.Primary.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers;

public class UserController : WebBaseController
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo(LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.SendAsync<LoginCommand, LoginResponse>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// 通过用户名获取用户信息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetUserInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo([FromQuery] GetUserInfoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.RequestAsync<GetUserInfoRequest, GetUserInfoResponse>(request, cancellationToken);
        return Ok(response);
    }
}