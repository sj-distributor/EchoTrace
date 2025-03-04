using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts;

/// <summary>
///  一个契约例子,可以删除
/// </summary>
public interface IGetUserInfoContract : IRequestContract<GetUserInfoRequest, GetUserInfoResponse>
{
    
}

public class GetUserInfoRequest : IRequest
{
    public string Username { get; set; }
}

public class GetUserInfoResponse : IResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public int Age { get; set; }
    public string PhoneNumber { get; set; }
}