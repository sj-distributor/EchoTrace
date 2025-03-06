using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Infrastructure.JwtFunction;
using EchoTrace.Primary.Contracts;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Realization.Bases;
using FluentValidation;
using Mediator.Net.Context;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EchoTrace.Realization.Handlers.Users;

public class LoginHandler(
    DbAccessor<ApplicationUser> userDbSet, IPasswordHasher passwordHasher,
    JwtService jwtService) : ILoginContract
{
    public async Task<LoginResponse> Handle(IReceiveContext<LoginCommand> context, CancellationToken cancellationToken)
    {
        var user = await userDbSet.DbSet.SingleOrDefaultAsync(x => x.UserName == context.Message.Username, cancellationToken);
        if (user == null)
        {
            throw new BusinessException("用户名或密码错误", BusinessExceptionTypeEnum.UnauthorizedIdentity);
        }
        
        if (!passwordHasher.VerifyHashedPasswordV3(user.PasswordHash, context.Message.Password))
        {
            throw new BusinessException("用户名或密码错误", BusinessExceptionTypeEnum.UnauthorizedIdentity);
        }

        var token = jwtService.GenerateAccessJwtToken(user.Id.ToString());
        var refreshToken = jwtService.GenerateRefreshJwtToken(token.Token);
        
        return new LoginResponse
        {
            AccessToken = token.Token,
            RefreshToken = refreshToken.Token
        };
    }
    
    public void Validate(ContractValidator<LoginCommand> validator)
    {
        validator.RuleFor(e => e.Username)
            .NotEmpty();

        validator.RuleFor(e => e.Password)
            .NotEmpty();
    }

    public void Test(TestContext<LoginCommand, LoginResponse> context)
    {
        var loginSuccessfullyCase = context.CreateTestCase();
        loginSuccessfullyCase.Message = new LoginCommand()
        {
            Username = "Zeke.Z",
            Password = "123456"
        };
        loginSuccessfullyCase.Arrange = async () =>
        {
            await context.DbContext.AddAsync(new ApplicationUser("Zeke.Z", passwordHasher.HashPasswordV3("123456")));
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