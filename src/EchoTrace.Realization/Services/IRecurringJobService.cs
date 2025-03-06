using EchoTrace.Infrastructure.Hangfire;
using EchoTrace.Realization.Bases;
using Hangfire;
using Hangfire.Server;

namespace EchoTrace.Realization.Services;

public interface IRecurringJobService
{
    Task AddOrUpdateAsync(RecurringJobInfo recurringJobInfo, string coreExpression, CancellationToken cancellationToken = default);

    Task RemoveIfExists(string jobId, CancellationToken cancellationToken = default);
}

[AsType(LifetimeEnum.Scope, typeof(IRecurringJobService))]
public class RecurringJobService : IRecurringJobService
{
    public async Task AddOrUpdateAsync(RecurringJobInfo recurringJobInfo, string coreExpression,
        CancellationToken cancellationToken = default)
    {
        RecurringJob.AddOrUpdate<IHangfireRegisterJobHelper>(recurringJobInfo.JobId,
            service => service.RunRecurringJob(recurringJobInfo,null), () => coreExpression);
        await Task.CompletedTask;
    }

    public async Task RemoveIfExists(string jobId, CancellationToken cancellationToken = default)
    {
         RecurringJob.RemoveIfExists(jobId);
         await Task.CompletedTask;
    }
}