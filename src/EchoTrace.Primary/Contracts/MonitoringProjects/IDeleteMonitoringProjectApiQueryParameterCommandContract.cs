using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IDeleteMonitoringProjectApiQueryParameterCommandContract : ICommandContract<DeleteMonitoringProjectApiQueryParameterCommand>
{
}

public class DeleteMonitoringProjectApiQueryParameterCommand : ICommand
{
    [FromRoute] 
    public Guid MonitoringProjectId { get; set; }

    [FromRoute]
    public Guid MonitoringProjectApiId { get; set; }
    [FromRoute]
    public Guid QueryParameterId { get; set; }
}