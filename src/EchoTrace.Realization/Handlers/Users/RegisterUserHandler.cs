using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Infrastructure.JwtFunction;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Primary.Contracts.Users;
using EchoTrace.Realization.Bases;
using FluentValidation;
using Mediator.Net.Context;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EchoTrace.Realization.Handlers.Users;

public class RegisterUserHandler(
    DbSet<ApplicationUser> applicationUserDbSet, IPasswordHasher passwordHasher) : IRegisterUserContract
{
    public async Task Handle(IReceiveContext<RegisterUserCommand> context, CancellationToken cancellationToken)
    {
        var user = await applicationUserDbSet.SingleOrDefaultAsync(x => x.UserName == context.Message.UserName,
            cancellationToken);
        if (user != null)
        {
            throw new BusinessException("当前用户名已存在", BusinessExceptionTypeEnum.DataDuplication);
        }

        var newUser = new ApplicationUser(context.Message.UserName,
            passwordHasher.HashPasswordV3(context.Message.Password));
        await applicationUserDbSet.AddAsync(newUser, cancellationToken);
    }
    
    public void Validate(ContractValidator<RegisterUserCommand> validator)
    {
        validator.RuleFor(x => x.UserName).NotEmpty();
        validator.RuleFor(x => x.Password).NotEmpty();
    }

    public void Test(TestContext<RegisterUserCommand> context)
    {
        var testCase = context.CreateTestCase();
        testCase.Message = new RegisterUserCommand
        {
            UserName = "Zeke.Z",
            Password = "123456"
        };
        testCase.Assert = async _ =>
        {
            var user = await context.DbContext.Set<ApplicationUser>().SingleOrDefaultAsync(x => x.UserName == "Zeke.Z");
            user.ShouldNotBeNull();
            user.UserName.ShouldBe("Zeke.Z");
            passwordHasher.VerifyHashedPasswordV3(user.PasswordHash,testCase.Message.Password).ShouldBeTrue();
        };
    }
}