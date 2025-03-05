using System.Text.RegularExpressions;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Primary.Contracts.MonitoringProjects;
using EchoTrace.Realization.Bases;
using FluentValidation;
using Mediator.Net.Context;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EchoTrace.Realization.Handlers.MonitoringProjects;

public class AddMonitoringProjectCommandHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    ICurrentSingle<ApplicationUser> currentUser) : IAddMonitoringProjectCommandContract
{
    public async Task Handle(IReceiveContext<AddMonitoringProjectCommand> context, CancellationToken cancellationToken)
    {
        var monitoringProjectDto = context.Message.MonitoringProject;
        var user = await currentUser.QueryAsync(cancellationToken);
        var monitoringProject = await monitoringProjectDbSet.DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == monitoringProjectDto.Name, cancellationToken);
        if (monitoringProject != null)
        {
            throw new BusinessException("The same project already exists.", BusinessExceptionTypeEnum.DataDuplication);
        }
        
        var newMonitoringProject = monitoringProjectDto.MapToSource(new MonitoringProject());
        await monitoringProjectDbSet.DbSet.AddAsync(newMonitoringProject, cancellationToken);
    }
    
    public void Validate(ContractValidator<AddMonitoringProjectCommand> validator)
    {
        validator.RuleFor(x => x.MonitoringProject).NotNull();
        validator.RuleFor(x => x.MonitoringProject.Name).NotEmpty();
        validator.RuleFor(x => x.MonitoringProject.BaseUrl).NotEmpty().Must(x =>
        {
            var isValidator = true;
            var pattern = @"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]";
            var match = Regex.Match(x, pattern);
            if (!match.Success)
            {
                isValidator = false;
            }
            return isValidator;
        }).WithMessage("The personal url format is incorrect.");
    }

    public void Test(TestContext<AddMonitoringProjectCommand> context)
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser("zeke", "123456")
        {
            Id = userId
        };
        var testCase = context.CreateTestCase();
        testCase.CurrentUser = user;
        testCase.Message = new AddMonitoringProjectCommand
        {
            MonitoringProject = new AddMonitoringProjectDto
            {
                Name = "Test",
                BaseUrl = "https://www.baidu.com"
            }
        };
        testCase.Arrange = async () => { await context.DbContext.AddAsync(user); };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            var monitoringProject =
                await context.DbContext.Set<MonitoringProject>().FirstOrDefaultAsync(x => x.Name == "Test");
            monitoringProject.ShouldNotBeNull();
            monitoringProject.Name.ShouldBe("Test");
            monitoringProject.BaseUrl.ShouldBe("https://www.baidu.com");
        };

        var errorTestCase = context.CreateTestCase();
        errorTestCase.CurrentUser = user;
        errorTestCase.Message = new AddMonitoringProjectCommand
        {
            MonitoringProject = new AddMonitoringProjectDto
            {
                Name = "Test",
                BaseUrl = "www.baidu.com"
            }
        };
        errorTestCase.Assert = async result =>
        {
            result.Exception.ShouldNotBeNull();
            result.Exception.ShouldBeOfType<BusinessException>();
            result.Exception.Message.ShouldBe("The personal url format is incorrect.");
            await Task.CompletedTask;
        };
    }
}