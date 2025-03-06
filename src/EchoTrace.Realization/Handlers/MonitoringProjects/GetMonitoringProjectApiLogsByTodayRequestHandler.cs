using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Primary.Contracts.MonitoringProjects;
using EchoTrace.Realization.Bases;
using Mediator.Net.Context;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EchoTrace.Realization.Handlers.MonitoringProjects;

public class GetMonitoringProjectApiLogsByTodayRequestHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet,
    DbAccessor<MonitoringProjectApiLog> monitoringProjectApiLogDbSet)
    : IGetMonitoringProjectApiLogsByTodayRequestContract
{
    public async Task<BaseResponse<GetMonitoringProjectApiLogsByTodayResponse>> Handle(
        IReceiveContext<GetMonitoringProjectApiLogsByTodayRequest> context, CancellationToken cancellationToken)
    {
        var utdNow = DateTime.UtcNow;
        var projects = await monitoringProjectDbSet.DbSet.AsNoTracking().ToListAsync(cancellationToken);
        var projectIds = projects.Select(x => x.Id).ToList();
        var projectApis = await monitoringProjectApiDbSet.DbSet.AsNoTracking()
            .Where(x => projectIds.Contains(x.MonitoringProjectId) && !x.IsDeactivate).ToListAsync(cancellationToken);
        var projectApiIds = projectApis.Select(x => x.Id).ToList();
        var projectApiLogs = await monitoringProjectApiLogDbSet.DbSet.AsNoTracking()
            .Where(x => x.CreatedOn.Date == utdNow.Date && projectApiIds.Contains(x.MonitoringProjectApiId)).ToListAsync(cancellationToken);
        var response = new GetMonitoringProjectApiLogsByTodayResponse
        {
            Projects = new List<GetMonitoringProjectApiLogsProjectDto>().MapFromSource(projects)
        };
        var projectApiDic = projectApis.GroupBy(x => x.MonitoringProjectId).ToDictionary(x => x.Key, x => x.ToList());
        var projectApiLogDic = projectApiLogs.GroupBy(x => x.MonitoringProjectApiId)
            .ToDictionary(x => x.Key, x => x.ToList());
        foreach (var project in response.Projects)
        {
            if (projectApiDic.TryGetValue(project.Id, out var value))
            {
                project.ProjectApis = new List<GetMonitoringProjectApiLogsProjectApiDto>().MapFromSource(value);
                project.ProjectApis = project.ProjectApis.Select(x => new GetMonitoringProjectApiLogsProjectApiDto
                {
                    Id = x.Id,
                    ApiName = x.ApiName,
                    ApiUrl = x.ApiUrl,
                    MonitorInterval = x.MonitorInterval,
                    ApiLogs = projectApiLogDic.TryGetValue(x.Id, out var logs) ? new List<GetMonitoringProjectApiLogsProjectApiLogDto>().MapFromSource(logs):[]
                }).ToList();
            }
        }

        return new BaseResponse<GetMonitoringProjectApiLogsByTodayResponse>(response);
    }

    public void Validate(ContractValidator<GetMonitoringProjectApiLogsByTodayRequest> validator)
    {
    }

    public void Test(
        TestContext<GetMonitoringProjectApiLogsByTodayRequest, BaseResponse<GetMonitoringProjectApiLogsByTodayResponse>>
            context)
    {
        var project = new MonitoringProject().Faker();
        var projectApis = new MonitoringProjectApi().Faker(2, x =>
        {
            x.MonitoringProjectId = project.Id;
            x.IsDeactivate = false;
        });
        var firstProjectLogs = new MonitoringProjectApiLog().Faker(2, x =>
        {
            x.MonitoringProjectApiId = projectApis.First().Id;
            x.HealthLevel = HealthLevel.Health;
            x.CreatedOn = DateTime.UtcNow;
        });
        var secondProjectLogs = new MonitoringProjectApiLog().Faker(2, x =>
        {
            x.MonitoringProjectApiId = projectApis.Last().Id;
            x.HealthLevel = HealthLevel.Dangerous;
            x.CreatedOn = DateTime.UtcNow;
        });

        var testCase = context.CreateTestCase();
        testCase.Message = new GetMonitoringProjectApiLogsByTodayRequest();
        testCase.Arrange = async () =>
        {
            await monitoringProjectDbSet.DbSet.AddAsync(project);
            await monitoringProjectApiDbSet.DbSet.AddRangeAsync(projectApis);
            await monitoringProjectApiLogDbSet.DbSet.AddRangeAsync(firstProjectLogs);
            await monitoringProjectApiLogDbSet.DbSet.AddRangeAsync(secondProjectLogs);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();

            var data = result.Response.Data;
            data.Projects.Count.ShouldBe(1);
            data.Projects.ForEach(x =>
            {
                x.Id.ShouldBe(project.Id);
                x.Name.ShouldBe(project.Name);
                x.BaseUrl.ShouldBe(project.BaseUrl);
                x.ProjectApis.Count.ShouldBe(2);
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).MonitorInterval
                    .ShouldBe(projectApis.First().MonitorInterval);
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).ApiName.ShouldBe(projectApis.First().ApiName);
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).ApiUrl.ShouldBe(projectApis.First().ApiUrl);
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).DangerousRate.ShouldBe("0%");
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).WarnRate.ShouldBe("0%");
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).HealthRate.ShouldBe("100%");
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).ApiLogs.Count.ShouldBe(2);
                x.ProjectApis.First(q => q.Id == projectApis.First().Id).ApiLogs.ForEach(y=>y.HealthLevel.ShouldBe(HealthLevel.Health));
            });

            await Task.CompletedTask;
        };
    }
}