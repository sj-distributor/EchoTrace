using EchoTrace.Infrastructure.DataPersistence.EfCore;
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

public class GetMonitoringProjectApiListByMonitoringProjectIdRequestHandler(
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet) : IGetMonitoringProjectApiListByMonitoringProjectIdRequestContract
{
    public async Task<BaseResponse<GetMonitoringProjectApiListByMonitoringProjectIdResponse>> Handle(IReceiveContext<GetMonitoringProjectApiListByMonitoringProjectIdRequest> context, CancellationToken cancellationToken)
    {
        var monitoringProject =
            await monitoringProjectDbSet.DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == context.Message.MonitoringProjectId,
                cancellationToken);
        if (monitoringProject == null)
        {
            throw new BusinessException("The project does not exist.", BusinessExceptionTypeEnum.DataNotExists);
        }

        var monitoringProjectApis = await monitoringProjectApiDbSet.DbSet.AsNoTracking()
            .Where(x => x.MonitoringProjectId == monitoringProject.Id).ToListAsync(cancellationToken);

        var dtoList = new List<GetMonitoringProjectApiDto>().MapFromSource(monitoringProjectApis);
        return new BaseResponse<GetMonitoringProjectApiListByMonitoringProjectIdResponse>(new GetMonitoringProjectApiListByMonitoringProjectIdResponse
        {
            MonitoringProjectApis = dtoList
        });
    }
    
    public void Validate(ContractValidator<GetMonitoringProjectApiListByMonitoringProjectIdRequest> validator)
    {
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
    }

    public void Test(TestContext<GetMonitoringProjectApiListByMonitoringProjectIdRequest, BaseResponse<GetMonitoringProjectApiListByMonitoringProjectIdResponse>> context)
    {
        var monitoringProjectId = Guid.NewGuid();

        var testCase = context.CreateTestCase();
        testCase.Message = new GetMonitoringProjectApiListByMonitoringProjectIdRequest
        {
            MonitoringProjectId = monitoringProjectId
        };
        testCase.Arrange = async () =>
        {
            var monitoringProject = new MonitoringProject().Faker(x=>x.Id = monitoringProjectId);
            var monitoringProjectApi = new MonitoringProjectApi().Faker(2, x =>
            {
                x.MonitoringProjectId = monitoringProjectId;
                x.CronExpression = "CronExpression";
                x.IsDeactivate = true;
                x.ApiUrl = "ApiUrl";
                x.BodyJson = "BodyJson";
            });

            await context.DbContext.AddAsync(monitoringProject);
            await context.DbContext.AddRangeAsync(monitoringProjectApi);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            var dtoList = result.Response.Data.MonitoringProjectApis;
            dtoList.ForEach(x =>
            {
                x.Id.ShouldNotBe(Guid.Empty);
                x.CronExpression.ShouldBe("CronExpression");
                x.IsDeactivate.ShouldBeTrue();
                x.ApiUrl.ShouldBe("ApiUrl");
                x.BodyJson.ShouldBe("BodyJson");
            });
            await Task.CompletedTask;
        };
    }
}