using Hangfire.States;
using Hangfire.Storage;

namespace JobHub.Infrastructure.Hangfire.HangfireStateHandlers;

public class SucceededJobExpireHandler(int succeededJobExpireTime) : IStateHandler
{
    public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var jobExpirationTimeout = TimeSpan.FromDays(succeededJobExpireTime);
        context.JobExpirationTimeout = jobExpirationTimeout;
    }

    public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    public string StateName => SucceededState.StateName;
}