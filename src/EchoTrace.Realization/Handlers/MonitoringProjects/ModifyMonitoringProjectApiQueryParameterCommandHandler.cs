using Autofac;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
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

public class ModifyMonitoringProjectApiQueryParameterCommandHandler(
    DbAccessor<MonitoringProjectApiRequestHeaderInfo> monitoringProjectApiRequestHeaderInfoDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet,
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApiQueryParameter> monitoringProjectApiQueryParameterDbSet,
    IRecurringJobService recurringJobService) : IModifyMonitoringProjectApiQueryParameterCommandContract
{
    public async Task Handle(IReceiveContext<ModifyMonitoringProjectApiQueryParameterCommand> context, CancellationToken cancellationToken)
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
            .Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id)
            .ToListAsync(cancellationToken);

        var monitoringProjectApiQueryParameter =
            monitoringProjectApiQueryParameters.First(x => x.Id == context.Message.QueryParameterId);
        context.Message.QueryParameter.MapToSource(monitoringProjectApiQueryParameter);
        monitoringProjectApiQueryParameterDbSet.DbSet.Update(monitoringProjectApiQueryParameter);
        
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
            }, monitoringProjectApi.CronExpression, cancellationToken); 
        }
        else
        {
            await recurringJobService.RemoveIfExists(jobId, cancellationToken);
        }
    }
    
    public void Validate(ContractValidator<ModifyMonitoringProjectApiQueryParameterCommand> validator)
    {
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
        validator.RuleFor(x => x.MonitoringProjectApiId).NotEmpty();
        validator.RuleFor(x => x.QueryParameterId).NotEmpty();
        validator.RuleFor(x => x.QueryParameter).NotNull();
        validator.RuleFor(x => x.QueryParameter.ParameterName).NotEmpty();
        validator.RuleFor(x => x.QueryParameter.ParameterValue).NotEmpty();
    }

    public void Test(TestContext<ModifyMonitoringProjectApiQueryParameterCommand> context)
    {
        var project = new MonitoringProject().Faker();
        var projectApi = new MonitoringProjectApi().Faker(x =>
        {
            x.MonitoringProjectId = project.Id;
            x.IsDeactivate = false;
        });
        var query =
            new MonitoringProjectApiQueryParameter().Faker(x => x.MonitoringProjectApiId = projectApi.Id);
        var testCase = context.CreateTestCase();
        testCase.Message = new ModifyMonitoringProjectApiQueryParameterCommand
        {
            MonitoringProjectId = project.Id,
            MonitoringProjectApiId = projectApi.Id,
            QueryParameterId = query.Id,
            QueryParameter = new ModifyMonitoringProjectApiQueryParameterDto
            {
                ParameterName = "ParameterName",
                ParameterValue = "ParameterValue"
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
            await context.DbContext.AddAsync(query);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            
            var monitoringProjectApiQueryParameter = await context.DbContext.Set<MonitoringProjectApiQueryParameter>()
                .FirstOrDefaultAsync(x => x.Id == testCase.Message.QueryParameterId);
            monitoringProjectApiQueryParameter.ShouldNotBeNull();
            monitoringProjectApiQueryParameter.ParameterName.ShouldBe(testCase.Message.QueryParameter.ParameterName);
            monitoringProjectApiQueryParameter.ParameterValue.ShouldBe(testCase.Message.QueryParameter.ParameterValue);
        };
    }
}