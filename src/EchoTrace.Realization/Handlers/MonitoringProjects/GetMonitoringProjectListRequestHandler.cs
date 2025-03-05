using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Primary.Contracts.MonitoringProjects;
using EchoTrace.Realization.Bases;
using Mediator.Net.Context;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EchoTrace.Realization.Handlers.MonitoringProjects;

public class GetMonitoringProjectListRequestHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet) : IGetMonitoringProjectListRequestContract
{
    public async Task<BaseResponse<GetMonitoringProjectListResponse>> Handle(IReceiveContext<GetMonitoringProjectListRequest> context, CancellationToken cancellationToken)
    {
        var monitoringProjectList = await monitoringProjectDbSet.DbSet.AsNoTracking().Where(x=> context.Message.Name == null || x.Name.Contains(context.Message.Name)).ToListAsync(cancellationToken);
        var response = new GetMonitoringProjectListResponse
        {
            MonitoringProjects = new List<GetMonitoringProjectDto>().MapFromSource(monitoringProjectList)
        };
        return new BaseResponse<GetMonitoringProjectListResponse>(response);
    }
    
    public void Validate(ContractValidator<GetMonitoringProjectListRequest> validator)
    {
    }

    public void Test(TestContext<GetMonitoringProjectListRequest, BaseResponse<GetMonitoringProjectListResponse>> context)
    {
        var monitoringProjectList = new MonitoringProject().Faker(2);
        var testCase = context.CreateTestCase();
        testCase.Message = new GetMonitoringProjectListRequest();
        testCase.Arrange = async () =>
        {
            await context.DbContext.AddRangeAsync(monitoringProjectList);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            result.Response.Data.MonitoringProjects.Count.ShouldBe(2);
            result.Response.Data.MonitoringProjects.First(x=>x.Id == monitoringProjectList.First().Id).Id.ShouldBe(monitoringProjectList.First().Id);
            result.Response.Data.MonitoringProjects.First(x=>x.Id == monitoringProjectList.First().Id).Name.ShouldBe(monitoringProjectList.First().Name);
            result.Response.Data.MonitoringProjects.First(x=>x.Id == monitoringProjectList.First().Id).BaseUrl.ShouldBe(monitoringProjectList.First().BaseUrl);
            await Task.CompletedTask;
        };

        var searchTestCase = context.CreateTestCase();
        searchTestCase.Message = new GetMonitoringProjectListRequest
        {
            Name = monitoringProjectList.First().Name
        };
        searchTestCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            result.Response.Data.MonitoringProjects.Count.ShouldBe(1);
            await Task.CompletedTask;
        };
    }
}