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

public class GetMonitoringProjectApiQueryParameterListRequestHandler(
    DbAccessor<MonitoringProjectApiQueryParameter> monitoringProjectApiQueryParameterDbSet,
    DbAccessor<MonitoringProject> monitoringProjectDbSet,
    DbAccessor<MonitoringProjectApi> monitoringProjectApiDbSet) : IGetMonitoringProjectApiQueryParameterListRequestContract
{
    public async Task<BaseResponse<GetMonitoringProjectApiQueryParameterListResponse>> Handle(
        IReceiveContext<GetMonitoringProjectApiQueryParameterListRequest> context, CancellationToken cancellationToken)
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

        var queryParameterList = await monitoringProjectApiQueryParameterDbSet.DbSet.AsNoTracking()
            .Where(x => x.MonitoringProjectApiId == monitoringProjectApi.Id).ToListAsync(cancellationToken);
        return new BaseResponse<GetMonitoringProjectApiQueryParameterListResponse>(new GetMonitoringProjectApiQueryParameterListResponse
        {
            QueryParameters = new List<GetMonitoringProjectApiQueryParameterDto>().MapFromSource(queryParameterList)
        });
    }
    
    public void Validate(ContractValidator<GetMonitoringProjectApiQueryParameterListRequest> validator)
    {
        validator.RuleFor(x => x.MonitoringProjectId).NotEmpty();
        validator.RuleFor(x => x.MonitoringProjectApiId).NotEmpty();
    }

    public void Test(TestContext<GetMonitoringProjectApiQueryParameterListRequest, BaseResponse<GetMonitoringProjectApiQueryParameterListResponse>> context)
    {
        var project = new MonitoringProject().Faker();
        var projectApi = new MonitoringProjectApi().Faker(x =>
        {
            x.MonitoringProjectId = project.Id;
            x.IsDeactivate = false;
        });
        var queryParameter = new MonitoringProjectApiQueryParameter().Faker(x => x.MonitoringProjectApiId = projectApi.Id);
        var testCase = context.CreateTestCase();
        testCase.Message = new GetMonitoringProjectApiQueryParameterListRequest
        {
            MonitoringProjectId = project.Id,
            MonitoringProjectApiId = projectApi.Id
        };
        testCase.Arrange = async () =>
        {
            await context.DbContext.AddAsync(project);
            await context.DbContext.AddAsync(projectApi);
            await context.DbContext.AddAsync(queryParameter);
        };
        testCase.Assert = async result =>
        {
            result.Exception.ShouldBeNull();

            var dtoList = result.Response.Data.QueryParameters;
            dtoList.ShouldNotBeNull();
            dtoList.Count.ShouldBe(1);
            dtoList.First().Id.ShouldBe(queryParameter.Id);
            dtoList.First().ParameterName.ShouldBe(queryParameter.ParameterName);
            dtoList.First().ParameterValue.ShouldBe(queryParameter.ParameterValue);
            await Task.CompletedTask;
        };
    }
}