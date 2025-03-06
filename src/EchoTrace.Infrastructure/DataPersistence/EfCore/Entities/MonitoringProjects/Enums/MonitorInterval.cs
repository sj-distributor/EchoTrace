namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;

public enum MonitorInterval
{
    OneMinute = 0,
    
    FiveMinutes = 10,
    
    TenMinutes = 20,
    
    ThirtyMinutes = 30,
    
    OneHour = 40,
    
    ThreeHours = 50,
    
    SixHours = 60,
    
    TwelveHours = 70,
    
    OneDay = 80
}

public static class MonitorIntervalExtensions
{
    public static string ToCronExpression(this MonitorInterval value)
    {
        return value switch
        {
            MonitorInterval.OneMinute => "* * * * *",
            MonitorInterval.FiveMinutes => "*/5 * * * *",
            MonitorInterval.TenMinutes => "*/10 * * * *",
            MonitorInterval.ThirtyMinutes => "*/30 * * * *",
            MonitorInterval.OneHour => "0 * * * *",
            MonitorInterval.ThreeHours => "0 */3 * * *",
            MonitorInterval.SixHours => "0 */6 * * *",
            MonitorInterval.TwelveHours => "0 */12 * * *",
            MonitorInterval.OneDay => "0 0 * * *",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}