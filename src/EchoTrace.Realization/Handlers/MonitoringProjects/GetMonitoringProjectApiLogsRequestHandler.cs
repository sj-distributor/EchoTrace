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

public class GetMonitoringProjectApiLogsRequestHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet,
    DbAccessor<MonitoringProjectApiLog> monitoringProjectApiLogDbSet)
    : IGetMonitoringProjectApiLogsRequestContract
{
    public async Task<BaseResponse<GetMonitoringProjectApiLogsResponse>> Handle(
        IReceiveContext<GetMonitoringProjectApiLogsRequest> context, CancellationToken cancellationToken)
    {
        var utdStartNow = DateTime.UtcNow.AddDays(-30).Date;
        var utdEndNow = DateTime.UtcNow.Date;
        var projects = await monitoringProjectDbSet.DbSet.AsNoTracking().ToListAsync(cancellationToken);
        var projectIds = projects.Select(x => x.Id).ToList();
        var projectApis = await monitoringProjectApiDbSet.DbSet.AsNoTracking()
            .Where(x => projectIds.Contains(x.MonitoringProjectId) && !x.IsDeactivate).ToListAsync(cancellationToken);
        var projectApiIds = projectApis.Select(x => x.Id).ToList();
        var projectApiLogs = await monitoringProjectApiLogDbSet.DbSet.AsNoTracking()
            .Where(x => utdStartNow <= x.CreatedOn.Date  && x.CreatedOn.Date <= utdEndNow && projectApiIds.Contains(x.MonitoringProjectApiId)).ToListAsync(cancellationToken);
        var response = new GetMonitoringProjectApiLogsResponse
        {
            Projects = new List<GetMonitoringProjectApiLogsProjectDto>().MapFromSource(projects)
        };
        var projectApiDic = projectApis.GroupBy(x => x.MonitoringProjectId).ToDictionary(x => x.Key, x => x.ToList());
        var projectApiLogDic = projectApiLogs
            .GroupBy(x => x.CreatedOn.Date) // 按日期分组
            .ToDictionary(
                dateGroup => dateGroup.Key, // 字典的键为日期
                dateGroup => dateGroup
                    .GroupBy(x => x.MonitoringProjectApiId) // 在每个日期内按 MonitoringProjectApiId 分组
                    .ToDictionary(
                        idGroup => idGroup.Key, // 字典的键为 MonitoringProjectApiId
                        idGroup => idGroup.ToList() // 值为该 ID 对应的日志列表
                    )
            );
        foreach (var project in response.Projects)
        {
            if (projectApiDic.TryGetValue(project.Id, out var value))
            {
                var projectApiDtoList = new List<GetMonitoringProjectApiLogsProjectApiDto>().MapFromSource(value);
                project.ProjectApis.AddRange(projectApiDtoList
                    .Select(x => new GetMonitoringProjectApiLogsProjectApiDto
                    {
                        Id = x.Id,
                        ApiName = x.ApiName,
                        ApiUrl = x.ApiUrl,
                        MonitorInterval = x.MonitorInterval
                    }).ToList());
                foreach (var key in projectApiLogDic.Keys)
                {
                    var dateValue = projectApiLogDic[key];
                    project.ProjectApis.ForEach(x =>
                    {
                        x.DayLogs.Add(new GetMonitoringProjectApiLogsProjectApiLogDayDto
                        {
                            Date = key,
                            ApiLogs = dateValue.TryGetValue(x.Id, out var logs) ? new List<GetMonitoringProjectApiLogsProjectApiLogDto>().MapFromSource(logs):[]
                        });
                    });
                }
            }
        }

        return new BaseResponse<GetMonitoringProjectApiLogsResponse>(response);
    }

    public void Validate(ContractValidator<GetMonitoringProjectApiLogsRequest> validator)
    {
    }

    public void Test(
        TestContext<GetMonitoringProjectApiLogsRequest, BaseResponse<GetMonitoringProjectApiLogsResponse>>
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
        testCase.Message = new GetMonitoringProjectApiLogsRequest();
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
            });

            await Task.CompletedTask;
        };
    }
}