using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;

namespace EchoTrace.Infrastructure.Hangfire;

public interface ISweepOutExpiredLogs
{
    Task SweepOutExpiredLogsAsync();
}

public class SweepOutExpiredLogs(ApplicationDbContext dbContext) : ISweepOutExpiredLogs
{
    public async Task SweepOutExpiredLogsAsync()
    {
        var utcNow = DateTime.UtcNow.AddDays(-30).Date;
        var logs = dbContext.Set<MonitoringProjectApiLog>().Where(x => x.CreatedOn.Date < utcNow);
        if (logs.Any())
        {
            dbContext.Set<MonitoringProjectApiLog>().RemoveRange(logs);
            await dbContext.SaveChangesAsync();
        }
    }
}