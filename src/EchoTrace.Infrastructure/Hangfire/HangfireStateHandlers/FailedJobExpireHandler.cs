using Hangfire.States;
using Hangfire.Storage;

namespace JobHub.Infrastructure.Hangfire.HangfireStateHandlers;

public class FailedJobExpireHandler(int failJobExpireTime) : IStateHandler
{
    public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var jobExpirationTimeout = TimeSpan.FromDays(failJobExpireTime);
        context.JobExpirationTimeout = jobExpirationTimeout;
    }

    public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        context.JobExpirationTimeout = TimeSpan.Zero;
    }

    public string StateName => FailedState.StateName;
}