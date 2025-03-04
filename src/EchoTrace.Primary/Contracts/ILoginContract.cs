using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts;

/// <summary>
///  一个契约例子，可以删除
/// </summary>
public interface ILoginContract : ICommandContract<LoginCommand, LoginResponse>
{
    
}

public class LoginCommand : ICommand
{
    public string Username { get; set; }

    public string Password { get; set; }
}

public class LoginResponse : IResponse
{
    /// <summary>
    /// 访问凭证
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 刷新凭证
    /// </summary>
    public string RefreshToken { get; set; }
}