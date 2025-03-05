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

public class AddMonitoringProjectApiCommandHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet,
    DbAccessor<MonitoringProjectApiRequestHeaderInfo> monitoringProjectApiRequestHeaderInfoDbSet,
    DbAccessor<MonitoringProjectApiQueryParameter> monitoringProjectApiQueryParameterDbSet,
    IRecurringJobService recurringJobService) : IAddMonitoringProjectApiCommandContract
{
    public async Task Handle(IReceiveContext<AddMonitoringProjectApiCommand> context, CancellationToken cancellationToken)
    {
        var monitoringProject =
            await monitoringProjectDbSet.DbSet.FirstOrDefaultAsync(x => x.Id == context.Message.MonitoringProjectId,
                cancellationToken);
        if (monitoringProject == null)
        {
            throw new BusinessException("The project does not exist.", BusinessExceptionTypeEnum.DataNotExists);
        }

        var monitoringProjectApi = await monitoringProjectApiDbSet.DbSet.FirstOrDefaultAsync(x =>
            x.MonitoringProjectId == context.Message.MonitoringProjectId && x.ApiName == context.Message.ApiName, cancellationToken);
        if (monitoringProjectApi != null)
        {
            throw new BusinessException("The project api already exists.", BusinessExceptionTypeEnum.DataDuplication);
        }
        
        var apiId = Guid.NewGuid();
        var jobId = monitoringProject.Name + "_" + context.Message.ApiName;
        var intactUrl = monitoringProject.BaseUrl + context.Message.ApiUrl;
        
        List<MonitoringProjectApiRequestHeaderInfo> monitoringProjectApiRequestHeaderInfos = [];
        if (context.Message.MonitoringProjectApiAdditionalInfo.MonitoringProjectApiRequestHeaderInfoList != null)
        {
            context.Message.MonitoringProjectApiAdditionalInfo.MonitoringProjectApiRequestHeaderInfoList.MapToSource(monitoringProjectApiRequestHeaderInfos);
            monitoringProjectApiRequestHeaderInfos.ForEach(x =>
            {
                x.MonitoringProjectApiId = apiId;
            });
        }
        
        List<MonitoringProjectApiQueryParameter> monitoringProjectApiQueryParameterList = [];
        if (context.Message.MonitoringProjectApiAdditionalInfo.MonitoringProjectApiQueryParameterList != null)
        {
            context.Message.MonitoringProjectApiAdditionalInfo.MonitoringProjectApiQueryParameterList.MapToSource(monitoringProjectApiQueryParameterList);
            monitoringProjectApiQueryParameterList.ForEach(x =>
            {
                x.MonitoringProjectApiId = apiId;
            });
        }

        await recurringJobService.AddOrUpdateAsync(new RecurringJobInfo
            {
                ApiId = apiId,
                Url = intactUrl,
                MonitoringProjectName = monitoringProject.Name,
                BodyJson = context.Message.BodyJson,
                HttpRequestMethod = context.Message.HttpRequestMethod,
                JobId = jobId,
                ExpectationStatusCode = context.Message.ExpectationCode,
                MonitoringProjectApiRequestHeaderInfos = monitoringProjectApiRequestHeaderInfos,
                MonitoringProjectApiQueryParameterList = monitoringProjectApiQueryParameterList
            }, context.Message.CronExpression,
            cancellationToken);

        var newMonitoringProjectApi = context.Message.MapToSource(new MonitoringProjectApi());
        newMonitoringProjectApi.Id = apiId;
        newMonitoringProjectApi.MonitoringProjectId = monitoringProject.Id;
        
        await monitoringProjectApiDbSet.DbSet.AddAsync(newMonitoringProjectApi, cancellationToken);
        
        if (monitoringProjectApiRequestHeaderInfos.Any())
        {
            await monitoringProjectApiRequestHeaderInfoDbSet.DbSet.AddRangeAsync(monitoringProjectApiRequestHeaderInfos, cancellationToken);
        }
        
        if (monitoringProjectApiQueryParameterList.Any())
        {
            await monitoringProjectApiQueryParameterDbSet.DbSet.AddRangeAsync(monitoringProjectApiQueryParameterList, cancellationToken);
        }
    }
    
    public void Validate(ContractValidator<AddMonitoringProjectApiCommand> validator)
    {
        validator.RuleFor(x => x.ApiUrl).NotEmpty();
        validator.RuleFor(x => x.ApiName).NotEmpty();
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
        validator.RuleFor(x => x.CronExpression).NotEmpty();
    }

    public void Test(TestContext<AddMonitoringProjectApiCommand> context)
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser("zeke", "123456")
        {
            Id = userId
        };
        var project = new MonitoringProject().Faker();
        var testCase = context.CreateTestCase();
        var message = new AddMonitoringProjectApiCommand
        {
            MonitoringProjectId = project.Id,
            ApiName = "test",
            ApiUrl = "/test",
            CronExpression = "*/1 * * * * *",
            HttpRequestMethod = HttpRequestMethod.Post,
            ExpectationCode = HttpStatusCode.OK,
            MonitoringProjectApiAdditionalInfo = new MonitoringProjectApiAdditionalInfo
            {
                MonitoringProjectApiQueryParameterList = [new MonitoringProjectApiQueryParameterDto
                    {
                        ParameterName = "Name",
                        ParameterValue = "Value"
                    }
                ],
                MonitoringProjectApiRequestHeaderInfoList = [new MonitoringProjectApiRequestHeaderInfoDto
                    {
                        RequestHeaderKey = "Key",
                        RequestHeaderValue = "Value"
                    }
                ]
            }
        };
        testCase.Message = message;
        testCase.CurrentUser = user;
        testCase.Build = builder =>
        {
            var jobService = Substitute.For<IRecurringJobService>();
            jobService.AddOrUpdateAsync(Arg.Any<RecurringJobInfo>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            builder.RegisterInstance(jobService);
        };
        testCase.Arrange = async () =>
        {
            await context.DbContext.AddAsync(user);
            await context.DbContext.AddAsync(project);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            
            var monitoringProjectApi = await context.DbContext.Set<MonitoringProjectApi>().FirstOrDefaultAsync(x => x.ApiName == "test");
            monitoringProjectApi.ShouldNotBeNull();
            monitoringProjectApi.ApiName.ShouldBe(message.ApiName);
            monitoringProjectApi.ApiUrl.ShouldBe(message.ApiUrl);
            monitoringProjectApi.CronExpression.ShouldBe(message.CronExpression);
            monitoringProjectApi.HttpRequestMethod.ShouldBe(message.HttpRequestMethod);
            monitoringProjectApi.ExpectationCode.ShouldBe(message.ExpectationCode);
            monitoringProjectApi.MonitoringProjectId.ShouldBe(project.Id);

            var monitoringProjectApiRequestHeaderInfo =
                await context.DbContext.Set<MonitoringProjectApiRequestHeaderInfo>().ToListAsync();
            monitoringProjectApiRequestHeaderInfo.ShouldNotBeNull();
            monitoringProjectApiRequestHeaderInfo.Count.ShouldBe(1);
            monitoringProjectApiRequestHeaderInfo.First().RequestHeaderKey.ShouldBe("Key");
            monitoringProjectApiRequestHeaderInfo.First().RequestHeaderValue.ShouldBe("Value");
            monitoringProjectApiRequestHeaderInfo.First().MonitoringProjectApiId.ShouldBe(monitoringProjectApi.Id);
            
            var monitoringProjectApiQueryParameter =
                await context.DbContext.Set<MonitoringProjectApiQueryParameter>().ToListAsync();
            monitoringProjectApiQueryParameter.ShouldNotBeNull();
            monitoringProjectApiQueryParameter.Count.ShouldBe(1);
            monitoringProjectApiQueryParameter.First().ParameterName.ShouldBe("Name");
            monitoringProjectApiQueryParameter.First().ParameterValue.ShouldBe("Value");
            monitoringProjectApiQueryParameter.First().MonitoringProjectApiId.ShouldBe(monitoringProjectApi.Id);
        };

        var errorTestCase = context.CreateTestCase();
        errorTestCase.DatabaseCleanupRequired = true;
        errorTestCase.Message = new AddMonitoringProjectApiCommand
        {
            MonitoringProjectId = project.Id,
            ApiName = "test",
            ApiUrl = "/test",
            CronExpression = "*/1 * * * * *",
            HttpRequestMethod = HttpRequestMethod.Post,
            ExpectationCode = HttpStatusCode.OK,
            MonitoringProjectApiAdditionalInfo = new MonitoringProjectApiAdditionalInfo()
        };
        errorTestCase.Assert = async result =>
        {
            result.Exception.ShouldNotBeNull();
            result.Exception.ShouldBeOfType<BusinessException>();
            result.Exception.Message.ShouldBe("The project does not exist.");
            await Task.CompletedTask;
        };
    }
}