using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IModifyMonitoringProjectApiRequestHeaderCommandContract : ICommandContract<ModifyMonitoringProjectApiRequestHeaderCommand>
{
}

public class ModifyMonitoringProjectApiRequestHeaderCommand : ICommand
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }

    [FromRoute]
    public Guid MonitoringProjectApiId { get; set; }
    
    [FromRoute]
    public Guid RequestHeaderInfoId { get; set; }
    
    [FromBody]
    public ModifyMonitoringProjectApiRequestHeaderDto ModifyMonitoringProjectApiRequestHeader { get; set; }
}

public class ModifyMonitoringProjectApiRequestHeaderDto : IMapFrom<MonitoringProjectApiRequestHeaderInfo>
{
    public string RequestHeaderKey { get; set; }
    
    public string RequestHeaderValue { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApiRequestHeaderInfo? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }
}