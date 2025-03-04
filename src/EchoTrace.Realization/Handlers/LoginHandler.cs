using EchoTrace.Primary.Contracts;
using EchoTrace.Primary.Contracts.Bases;
using FluentValidation;
using Mediator.Net.Context;
using Shouldly;

namespace EchoTrace.Realization.Handlers;

/// <summary>
/// 一个契约的实现的例子，可以删除
/// </summary>
public class LoginHandler : ILoginContract
{
    public void Validate(ContractValidator<LoginCommand> validator)
    {
        validator.RuleFor(e => e.Username)
            .NotEmpty();

        validator.RuleFor(e => e.Password)
            .NotEmpty();
    }

    public Task<LoginResponse> Handle(IReceiveContext<LoginCommand> context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new LoginResponse()
        {
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString()
        });
    }

    public void Test(TestContext<LoginCommand, LoginResponse> context)
    {
        // 默认需要数据库，不需要数据库可以手动设置
        context.NoDatabase = true;

        var loginSuccessfullyCase = context.CreateTestCase();
        loginSuccessfullyCase.Build = builder =>
        {
            //mock service
        };
        loginSuccessfullyCase.Message = new LoginCommand()
        {
            Username = "username",
            Password = "password"
        };
        loginSuccessfullyCase.Arrange = async () =>
        {
            // arrange action
            await Task.CompletedTask;
        };

        loginSuccessfullyCase.Assert = result =>
        {
            result.Exception.ShouldBeNull();
            result.Response.AccessToken.ShouldNotBeNullOrWhiteSpace();
            result.Response.RefreshToken.ShouldNotBeNullOrWhiteSpace();
            return Task.CompletedTask;
        };
    }
}