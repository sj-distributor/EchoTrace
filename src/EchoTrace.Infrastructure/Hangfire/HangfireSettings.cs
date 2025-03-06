using EchoTrace.Infrastructure.Bases;

namespace EchoTrace.Infrastructure.Hangfire;

public class HangfireSettings : IJsonFileSetting
{
    public int RedisDb { get; set; } = 10;

    public int SucceededJobExpireTime { get; set; } = 3;
    
    public int FailedJobExpireTime { get; set; } = 3;

    public string RedisConnectionString { get; set; } = string.Empty;
    
    public string EnableHangfireDashboard { get; set; } = string.Empty;
    
    public string JsonFilePath => "./Hangfire/Hangfire-settings.json";

    public int JobMemoryCacheTimeout { get; set; } = 5;

    public string UserName { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;

    public string RobotUrl { get; set; } = string.Empty;
}