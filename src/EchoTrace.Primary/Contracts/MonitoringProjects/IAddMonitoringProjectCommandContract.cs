using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IAddMonitoringProjectCommandContract : ICommandContract<AddMonitoringProjectCommand>
{
}

public class AddMonitoringProjectCommand : ICommand
{
    [FromQuery]
    public AddMonitoringProjectDto MonitoringProject { get; set; }
}

public class AddMonitoringProjectDto : IMapFrom<MonitoringProject>
{
    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProject? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }
}