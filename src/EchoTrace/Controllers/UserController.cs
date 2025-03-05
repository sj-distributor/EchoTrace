using EchoTrace.Controllers.Bases;
using EchoTrace.Primary.Contracts;
using EchoTrace.Primary.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo(LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.SendAsync<LoginCommand, LoginResponse>(command, cancellationToken);
        return Ok(response);
    }
    
    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns></returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(RegisterUserCommand command) 
    {
        await Mediator.SendAsync(command);
        return Ok();
    }
}