using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IAddMonitoringProjectApiQueryParameterCommandContract : ICommandContract<AddMonitoringProjectApiQueryParameterCommand>
{
}

public class AddMonitoringProjectApiQueryParameterCommand : ICommand
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }

    [FromRoute]
    public Guid MonitoringProjectApiId { get; set; }
    
    [FromBody]
    public AddMonitoringProjectApiQueryParameterDto AddMonitoringProjectApiQueryParameter { get; set; }
}

public class AddMonitoringProjectApiQueryParameterDto : IMapFrom<MonitoringProjectApiQueryParameter>
{
    public string ParameterName { get; set; }

    public string ParameterValue { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApiQueryParameter? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }
}