using System.Net;
using Autofac;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using EchoTrace.Infrastructure.Hangfire;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Primary.Contracts.MonitoringProjects;
using EchoTrace.Realization.Bases;
using EchoTrace.Realization.Services;
using FluentValidation;
using Mediator.Net.Context;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace EchoTrace.Realization.Handlers.MonitoringProjects;

public class ModifyMonitoringProjectApiCommandHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet,DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet,
    DbAccessor<MonitoringProjectApiQueryParameter> monitoringProjectApiQueryParameterDbSet,
    DbAccessor<MonitoringProjectApiRequestHeaderInfo> monitoringProjectApiRequestHeaderInfoDbSet,
    IRecurringJobService recurringJobService) : IModifyMonitoringProjectApiCommandContract
{
    public async Task Handle(IReceiveContext<ModifyMonitoringProjectApiCommand> context, CancellationToken cancellationToken)
    {
        var monitoringProject = await monitoringProjectDbSet.DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.Message.MonitoringProjectId, cancellationToken);
        if (monitoringProject == null)
        {
            throw new BusinessException("The project does not exist.", BusinessExceptionTypeEnum.DataNotExists);
        }

        var monitoringProjectApi =
            await monitoringProjectApiDbSet.DbSet.FirstOrDefaultAsync(
                x => x.Id == context.Message.MonitoringProjectApiId, cancellationToken);
        if (monitoringProjectApi == null)
        {
            throw new BusinessException("The project api does not exist.", BusinessExceptionTypeEnum.DataNotExists);
        }

        context.Message.ModifyMonitoringProjectApi.MapToSource(monitoringProjectApi);
        var apiId = monitoringProjectApi.Id;
        var jobId = monitoringProject.Name + "_" + monitoringProjectApi.ApiName;
        var intactUrl = monitoringProject.BaseUrl + monitoringProjectApi.ApiUrl;
        var monitoringProjectApiRequestHeaderInfos = await monitoringProjectApiRequestHeaderInfoDbSet.DbSet
            .AsNoTracking().Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id)
            .ToListAsync(cancellationToken);
        var monitoringProjectApiQueryParameters = await monitoringProjectApiQueryParameterDbSet.DbSet
            .AsNoTracking().Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id)
            .ToListAsync(cancellationToken);
        if (!monitoringProjectApi.IsDeactivate)
        {
            await recurringJobService.AddOrUpdateAsync(new RecurringJobInfo
            {
                ApiId = apiId,
                Url = intactUrl,
                MonitoringProjectName = monitoringProject.Name,
                BodyJson = monitoringProjectApi.BodyJson,
                HttpRequestMethod = monitoringProjectApi.HttpRequestMethod,
                JobId = jobId,
                ExpectationStatusCode = monitoringProjectApi.ExpectationCode,
                MonitoringProjectApiRequestHeaderInfos = monitoringProjectApiRequestHeaderInfos,
                MonitoringProjectApiQueryParameterList = monitoringProjectApiQueryParameters
            }, monitoringProjectApi.MonitorInterval.ToCronExpression(), cancellationToken); 
        }
        else
        {
            await recurringJobService.RemoveIfExists(jobId, cancellationToken);
        }
        
        monitoringProjectApiDbSet.DbSet.Update(monitoringProjectApi);
    }
    
    public void Validate(ContractValidator<ModifyMonitoringProjectApiCommand> validator)
    {
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
        validator.RuleFor(x => x.MonitoringProjectApiId).NotEmpty();
        validator.RuleFor(x => x.ModifyMonitoringProjectApi.MonitorInterval).NotNull();
    }

    public void Test(TestContext<ModifyMonitoringProjectApiCommand> context)
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser("zeke", "123456")
        {
            Id = userId
        };
        var project = new MonitoringProject().Faker();
        var api = new MonitoringProjectApi().Faker(x =>
        {
            x.MonitoringProjectId = project.Id;
            x.IsDeactivate = true;
        });

        var testCase = context.CreateTestCase();
        testCase.CurrentUser = user;
        testCase.Message = new ModifyMonitoringProjectApiCommand
        {
            MonitoringProjectId = project.Id,
            MonitoringProjectApiId = api.Id,
            ModifyMonitoringProjectApi = new ModifyMonitoringProjectApiDto
            {
                MonitorInterval = MonitorInterval.OneMinute,
                HttpRequestMethod = HttpRequestMethod.Post,
                IsDeactivate = false,
                ExpectationCode = HttpStatusCode.OK,
                BodyJson = "BodyJson"
            }
        };
        testCase.Build = builder =>
        {
            var jobService = Substitute.For<IRecurringJobService>();
            jobService.AddOrUpdateAsync(Arg.Any<RecurringJobInfo>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            builder.RegisterInstance(jobService);;
        };
        testCase.Arrange = async () =>
        {
            await context.DbContext.AddAsync(user);
            await context.DbContext.AddAsync(project);
            await context.DbContext.AddAsync(api);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();

            var dbApi = await context.DbContext.Set<MonitoringProjectApi>().FirstAsync();
            dbApi.ShouldNotBeNull();
            dbApi.MonitorInterval.ShouldBe(MonitorInterval.OneMinute);
            dbApi.HttpRequestMethod.ShouldBe(HttpRequestMethod.Post);
            dbApi.ExpectationCode.ShouldBe(HttpStatusCode.OK);
            dbApi.BodyJson.ShouldBe("BodyJson");
            dbApi.IsDeactivate.ShouldBeFalse();
        };
    }
}