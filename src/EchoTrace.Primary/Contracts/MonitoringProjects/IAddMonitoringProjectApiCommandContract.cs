using System.Net;
using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IAddMonitoringProjectApiCommandContract : ICommandContract<AddMonitoringProjectApiCommand>
{
}

public class AddMonitoringProjectApiCommand : ICommand, IMapFrom<MonitoringProjectApi>
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }
    
    public string ApiName { get; set; }
    
    /// <summary>
    ///  ps: /health
    /// </summary>
    public string ApiUrl { get; set; }
    
    /// <summary>
    ///   0：OneMinute， 10：FiveMinutes， 20：TenMinutes， 30：ThirtyMinutes， 40：OneHour， 50：ThreeHours， 60：SixHours， 70：TwelveHours， 80：OneDay
    /// </summary>
    public MonitorInterval MonitorInterval { get; set; }
    
    /// <summary>
    /// 0.Post 1.Patch 2.Put 3.Delete
    /// </summary>
    public HttpRequestMethod HttpRequestMethod { get; set; }
    
    public bool IsDeactivate { get; set; }

    /// <summary>
    ///  ps: {"name":"zeke.z","age":18}
    /// </summary>
    public string? BodyJson { get; set; }
    
    public HttpStatusCode ExpectationCode { get; set; }
    
    /// <summary>
    ///  请求参数和请求头信息
    /// </summary>
    [FromBody]
    public MonitoringProjectApiAdditionalInfo MonitoringProjectApiAdditionalInfo { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApi? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }
}

public class MonitoringProjectApiRequestHeaderInfoDto : IMapFrom<MonitoringProjectApiRequestHeaderInfo>
{
    public string RequestHeaderKey { get; set; }
    
    public string RequestHeaderValue { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApiRequestHeaderInfo? source)
    {
        this.CreateMapperConfiguration(cfg, source)
            .ReverseMap();
    }
}

public class MonitoringProjectApiQueryParameterDto : IMapFrom<MonitoringProjectApiQueryParameter>
{
    public string ParameterName { get; set; }
    
    public string ParameterValue { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApiQueryParameter? source)
    {
        this.CreateMapperConfiguration(cfg, source)
            .ReverseMap();
    }
}

public class MonitoringProjectApiAdditionalInfo
{
    /// <summary>
    ///  请求 Query参数
    /// </summary>
    [FromBody]
    public List<MonitoringProjectApiQueryParameterDto>? MonitoringProjectApiQueryParameterList { get; set; } = [];
    
    /// <summary>
    ///  请求头信息
    /// </summary>
    [FromBody]
    public List<MonitoringProjectApiRequestHeaderInfoDto> MonitoringProjectApiRequestHeaderInfoList { get; set; } = [];
}