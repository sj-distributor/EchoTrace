using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IGetMonitoringProjectApiLogsRequestContract : 
    IRequestContract<GetMonitoringProjectApiLogsRequest, BaseResponse<GetMonitoringProjectApiLogsResponse>>
{
}

public class GetMonitoringProjectApiLogsRequest : IRequest
{
}

public class GetMonitoringProjectApiLogsResponse
{
    public List<GetMonitoringProjectApiLogsProjectDto> Projects { get; set; } = [];
}

public class GetMonitoringProjectApiLogsProjectDto : IMapFrom<MonitoringProject>
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public List<GetMonitoringProjectApiLogsProjectApiDto> ProjectApis { get; set; } = [];

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProject? source)
    {
        this.CreateMapperConfiguration(cfg, source)
            .ForMember(des => des.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(des => des.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(des => des.BaseUrl, opt => opt.MapFrom(src => src.BaseUrl))
            .ReverseMap();
    }
}

public class GetMonitoringProjectApiLogsProjectApiDto : IMapFrom<MonitoringProjectApi>
{
    public Guid Id { get; set; }

    public string ApiName { get; set; }
    
    public string ApiUrl { get; set; }

    public string HealthRate => ComputeHealthRate(HealthLevel.Health);

    public string WarnRate => ComputeHealthRate(HealthLevel.Warn);
    
    public string DangerousRate => ComputeHealthRate(HealthLevel.Dangerous);
    
    public MonitorInterval MonitorInterval { get; set; }

    public List<GetMonitoringProjectApiLogsProjectApiLogDayDto> DayLogs { get; set; } = [];

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApi? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }

    private string ComputeHealthRate(HealthLevel healthLevel)
    {
        var count = DayLogs.Count;
        if (count == 0)
        {
            return "0%";
        }
        var healthCount = DayLogs.Count(x => x.HealthLevel == healthLevel);
        var rate = ((double)healthCount / count) * 100;
        return $"{rate}%";
    }
}

public class GetMonitoringProjectApiLogsProjectApiLogDayDto
{
    public DateTime Date { get; set; }

    public HealthLevel HealthLevel => ComputeHealthLevel();

    public List<GetMonitoringProjectApiLogsProjectApiLogDto> ApiLogs { get; set; } = [];

    private HealthLevel ComputeHealthLevel()
    {
        var count = ApiLogs.Count;
        if (count == 0)
        {
            return HealthLevel.Health;
        }

        var healthCount = ApiLogs.Select(x => x.HealthLevel == HealthLevel.Health).Count();

        var healthRate = (double)healthCount / count * 100;

        var healthLevel = healthRate switch
        {
            >= 90 => HealthLevel.Health,
            > 10 and <= 90 => HealthLevel.Warn,
            _ => HealthLevel.Dangerous
        };
        return healthLevel;
    }
}

public class GetMonitoringProjectApiLogsProjectApiLogDto : IMapFrom<MonitoringProjectApiLog>
{
    public Guid Id { get; set; }

    public HealthLevel HealthLevel { get; set; }

    public DateTime CreatedOn { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApiLog? source)
    {
        this.CreateMapperConfiguration(cfg, source).ReverseMap();
    }
}