using System.Net;
using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IGetMonitoringProjectApiListByMonitoringProjectIdRequestContract :
    IRequestContract<GetMonitoringProjectApiListByMonitoringProjectIdRequest, BaseResponse<GetMonitoringProjectApiListByMonitoringProjectIdResponse>>
{
}

public class GetMonitoringProjectApiListByMonitoringProjectIdRequest : IRequest
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }
}

public class GetMonitoringProjectApiListByMonitoringProjectIdResponse
{
    public List<GetMonitoringProjectApiDto> MonitoringProjectApis { get; set; } = [];
}

public class GetMonitoringProjectApiDto :IMapFrom<MonitoringProjectApi>
{
    public Guid Id { get; set; }
    
    public string ApiName { get; set; }
    
    public string ApiUrl { get; set; }

    public string? BodyJson { get; set; }
    
    public HttpRequestMethod HttpRequestMethod { get; set; }

    public bool IsDeactivate { get; set; }

    public string CronExpression { get; set; }
    
    public HttpStatusCode ExpectationCode { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApi? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }
}