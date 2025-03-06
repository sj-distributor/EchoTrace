using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.Users;

public interface IRegisterUserContract : ICommandContract<RegisterUserCommand>
{
}

public class RegisterUserCommand : ICommand
{
    public string UserName { get; set; }
    
    public string Password { get; set; }
}