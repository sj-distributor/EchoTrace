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

public class GetMonitoringProjectApiRequestHeaderListRequestHandler(
    DbAccessor<MonitoringProjectApiRequestHeaderInfo> monitoringProjectApiRequestHeaderInfoDbSet,
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet) : IGetMonitoringProjectApiRequestHeaderListRequestContract
{
    public async Task<BaseResponse<GetMonitoringProjectApiRequestHeaderListResponse>> Handle(IReceiveContext<GetMonitoringProjectApiRequestHeaderListRequest> context, CancellationToken cancellationToken)
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
        
        var monitoringProjectApiRequestHeaderInfos = await monitoringProjectApiRequestHeaderInfoDbSet.DbSet
            .AsNoTracking().Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id)
            .ToListAsync(cancellationToken);

        return new BaseResponse<GetMonitoringProjectApiRequestHeaderListResponse>(
            new GetMonitoringProjectApiRequestHeaderListResponse
            {
                MonitoringProjectApiRequestHeaderInfos = new List<GetMonitoringProjectApiRequestHeaderDto>().MapFromSource(monitoringProjectApiRequestHeaderInfos)
            });

    }
    
    public void Validate(ContractValidator<GetMonitoringProjectApiRequestHeaderListRequest> validator)
    {
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
        validator.RuleFor(x => x.MonitoringProjectApiId).NotEmpty();
    }

    public void Test(TestContext<GetMonitoringProjectApiRequestHeaderListRequest, BaseResponse<GetMonitoringProjectApiRequestHeaderListResponse>> context)
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
        testCase.Message = new GetMonitoringProjectApiRequestHeaderListRequest
        {
            MonitoringProjectId = project.Id,
            MonitoringProjectApiId = projectApi.Id
        };
        testCase.Arrange = async () =>
        {
            await monitoringProjectDbSet.DbSet.AddAsync(project);
            await monitoringProjectApiDbSet.DbSet.AddAsync(projectApi);
            await monitoringProjectApiRequestHeaderInfoDbSet.DbSet.AddAsync(requestHeader);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();
            var dtoList = result.Response.Data.MonitoringProjectApiRequestHeaderInfos;
            dtoList.Count.ShouldBe(1);
            dtoList.ForEach(x =>
            {
                x.Id.ShouldNotBe(Guid.Empty);
                x.RequestHeaderKey.ShouldBe(requestHeader.RequestHeaderKey);
                x.RequestHeaderValue.ShouldBe(requestHeader.RequestHeaderValue);
            });
            await Task.CompletedTask;
        };
    }
}