using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore;

public class DbAccessor<T>(ApplicationDbContext context) where T : class, IEfEntity<T>
{
    public DbSet<T> DbSet => context.Set<T>();
}