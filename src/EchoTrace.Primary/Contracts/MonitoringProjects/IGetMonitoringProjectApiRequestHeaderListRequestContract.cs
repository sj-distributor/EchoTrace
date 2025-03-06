using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IGetMonitoringProjectApiRequestHeaderListRequestContract :
    IRequestContract<GetMonitoringProjectApiRequestHeaderListRequest, BaseResponse<GetMonitoringProjectApiRequestHeaderListResponse>>
{
}

public class GetMonitoringProjectApiRequestHeaderListRequest : IRequest
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }

    [FromRoute]
    public Guid MonitoringProjectApiId { get; set; }
}

public class GetMonitoringProjectApiRequestHeaderListResponse
{
    public List<GetMonitoringProjectApiRequestHeaderDto> MonitoringProjectApiRequestHeaderInfos { get; set; } = new();
}

public class GetMonitoringProjectApiRequestHeaderDto : IMapFrom<MonitoringProjectApiRequestHeaderInfo>
{
    public Guid Id { get; set; }
    
    public string RequestHeaderKey { get; set; }
    
    public string RequestHeaderValue { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApiRequestHeaderInfo? source)
    {
        this.CreateMapperConfiguration(cfg, source)
            .ReverseMap();
    }
}