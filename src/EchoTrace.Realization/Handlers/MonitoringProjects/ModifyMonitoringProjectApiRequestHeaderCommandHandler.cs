using Autofac;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
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

public class ModifyMonitoringProjectApiRequestHeaderCommandHandler(
    DbAccessor<MonitoringProjectApiRequestHeaderInfo> monitoringProjectApiRequestHeaderInfoDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet,
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApiQueryParameter> monitoringProjectApiQueryParameterDbSet,
    IRecurringJobService recurringJobService) : IModifyMonitoringProjectApiRequestHeaderCommandContract
{
    public async Task Handle(IReceiveContext<ModifyMonitoringProjectApiRequestHeaderCommand> context, CancellationToken cancellationToken)
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
        
        var monitoringProjectApiRequestHeaderInfos = await monitoringProjectApiRequestHeaderInfoDbSet.DbSet.Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id)
            .ToListAsync(cancellationToken);
        var monitoringProjectApiQueryParameters = await monitoringProjectApiQueryParameterDbSet.DbSet
            .AsNoTracking().Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id)
            .ToListAsync(cancellationToken);
        
        var monitoringProjectApiRequestHeaderInfo =
            monitoringProjectApiRequestHeaderInfos.First(x => x.Id == context.Message.RequestHeaderInfoId);
        context.Message.ModifyMonitoringProjectApiRequestHeader.MapToSource(monitoringProjectApiRequestHeaderInfo);
        monitoringProjectApiRequestHeaderInfoDbSet.DbSet.Update(monitoringProjectApiRequestHeaderInfo);

        var apiId = monitoringProjectApi.Id;
        var jobId = monitoringProject.Name + "_" + monitoringProjectApi.ApiName;
        var intactUrl = monitoringProject.BaseUrl + monitoringProjectApi.ApiUrl;
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
    }
    
    public void Validate(ContractValidator<ModifyMonitoringProjectApiRequestHeaderCommand> validator)
    {
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
        validator.RuleFor(x => x.MonitoringProjectApiId).NotEmpty();
        validator.RuleFor(x => x.RequestHeaderInfoId).NotEmpty();
        validator.RuleFor(x => x.ModifyMonitoringProjectApiRequestHeader.RequestHeaderValue).NotEmpty();
        validator.RuleFor(x => x.ModifyMonitoringProjectApiRequestHeader.RequestHeaderKey).NotEmpty();
    }

    public void Test(TestContext<ModifyMonitoringProjectApiRequestHeaderCommand> context)
    {
        var project = new MonitoringProject().Faker();
        var projectApi = new MonitoringProjectApi().Faker(x =>
        {
            x.MonitoringProjectId = project.Id;
            x.IsDeactivate = false;
        });
        var requestHeader =
            new MonitoringProjectApiRequestHeaderInfo().Faker(x => x.MonitoringProjectApiId = projectApi.Id);
        var testCase = context.CreateTestCase();
        testCase.Message = new ModifyMonitoringProjectApiRequestHeaderCommand
        {
            MonitoringProjectId = project.Id,
            MonitoringProjectApiId = projectApi.Id,
            RequestHeaderInfoId = requestHeader.Id,
            ModifyMonitoringProjectApiRequestHeader = new ModifyMonitoringProjectApiRequestHeaderDto
            {
                RequestHeaderKey = "Key",
                RequestHeaderValue = "Value"
            }
        };
        testCase.Build = builder =>
        {
            var jobService = Substitute.For<IRecurringJobService>();
            jobService.AddOrUpdateAsync(Arg.Any<RecurringJobInfo>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            builder.RegisterInstance(jobService);
        };
        testCase.Arrange = async () =>
        {
            await context.DbContext.AddAsync(project);
            await context.DbContext.AddAsync(projectApi);
            await context.DbContext.AddAsync(requestHeader);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            
            var monitoringProjectApiRequestHeaderInfo = await context.DbContext.Set<MonitoringProjectApiRequestHeaderInfo>()
                .FirstOrDefaultAsync(x => x.Id == testCase.Message.RequestHeaderInfoId);
            monitoringProjectApiRequestHeaderInfo.ShouldNotBeNull();
            monitoringProjectApiRequestHeaderInfo.RequestHeaderKey.ShouldBe(testCase.Message.ModifyMonitoringProjectApiRequestHeader.RequestHeaderKey);
            monitoringProjectApiRequestHeaderInfo.RequestHeaderValue.ShouldBe(testCase.Message.ModifyMonitoringProjectApiRequestHeader.RequestHeaderValue);
        };
    }
}