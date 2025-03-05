using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IGetMonitoringProjectApiQueryParameterListRequestContract : IRequestContract<GetMonitoringProjectApiQueryParameterListRequest, BaseResponse<GetMonitoringProjectApiQueryParameterListResponse>>
{
}

public class GetMonitoringProjectApiQueryParameterListRequest : IRequest
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }
    
    [FromRoute]
    public Guid MonitoringProjectApiId { get; set; }
}

public class GetMonitoringProjectApiQueryParameterListResponse
{
    public List<GetMonitoringProjectApiQueryParameterDto>? QueryParameters { get; set; }
}

public class GetMonitoringProjectApiQueryParameterDto : IMapFrom<MonitoringProjectApiQueryParameter>
{
    public Guid Id { get; set; }

    public string ParameterName { get; set; }
    
    public string ParameterValue { get; set; }
}