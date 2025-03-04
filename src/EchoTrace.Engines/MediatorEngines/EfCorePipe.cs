using Autofac;
using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Primary.Bases;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Engines.MediatorEngines;

public class EfCorePipe(ILifetimeScope lifetimeScope) : IPipeSpecification<IReceiveContext<IMessage>>
{
    private readonly ApplicationDbContext? _dbContext = lifetimeScope.Resolve<ApplicationDbContext>();
    
    private readonly ICurrentSingle<ApplicationUser> _applicationUser =
        lifetimeScope.Resolve<ICurrentSingle<ApplicationUser>>();

    public bool ShouldExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        return true;
    }

    public Task BeforeExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        return Task.WhenAll();
    }

    public Task Execute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        return Task.WhenAll();
    }

    public async Task AfterExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        if (_dbContext is { ChangeTracker: not null })
        {
            if (_dbContext.ChangeTracker.HasChanges())
            {
                await HandleEntityAuditingBeforeSaveChangesAsync(_dbContext);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public Task OnException(Exception ex, IReceiveContext<IMessage> context)
    {
        throw ex;
    }
    
    private async Task HandleEntityAuditingBeforeSaveChangesAsync(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IHasCreator objectWithCreator)
                    {
                        if (objectWithCreator.CreatedBy == default)
                        {
                            objectWithCreator.CreatedBy = await _applicationUser.GetCurrentUserIdAsync();
                        }
                    }

                    break;

                case EntityState.Modified:
                    if (entry.Entity is IHasUpdater { UpdatedOn : null } objectWithUpdateOn)
                    {
                        objectWithUpdateOn.UpdatedOn = DateTime.UtcNow;
                        objectWithUpdateOn.UpdatedBy = await _applicationUser.GetCurrentUserIdAsync();
                    }

                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ICanSoftDelete objectWithSoftDelete)
                    {
                        objectWithSoftDelete.IsDeleted = true;
                        entry.State = EntityState.Modified;
                    }

                    break;
            }
        }
    }
}