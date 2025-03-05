using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IGetMonitoringProjectListRequestContract : IRequestContract<GetMonitoringProjectListRequest, BaseResponse<GetMonitoringProjectListResponse>>
{
}

public class GetMonitoringProjectListRequest : IRequest
{
    public string? Name { get; set; }
}

public class GetMonitoringProjectListResponse
{
    public List<GetMonitoringProjectDto> MonitoringProjects { get; set; } = new();
}

public class GetMonitoringProjectDto : IMapFrom<MonitoringProject>
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string BaseUrl { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProject? source)
    {
        this.CreateMapperConfiguration(cfg, source)
            .ReverseMap();
    }
}