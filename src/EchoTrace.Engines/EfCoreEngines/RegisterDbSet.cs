using Autofac;
using EchoTrace.Engines.Bases;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Engines.EfCoreEngines;
public class RegisterDbSet : IBuilderEngine
{
    private readonly ContainerBuilder _container;

    public RegisterDbSet(ContainerBuilder container)
    {
        _container = container;
    }

    public void Run()
    {
        _container.RegisterType<ApplicationDbContext>()
            .AsSelf()
            .As<DbContext>()
            .InstancePerLifetimeScope();
        var idbEntityType = typeof(IEfEntity<>);
        var idbEntityAssembly = idbEntityType.Assembly;
        var dbEntityTypes = idbEntityAssembly
            ?.ExportedTypes
            .Where(e => e.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == idbEntityType) &&
                        e is { IsClass: true, IsAbstract: false })
            .ToArray();
        if (dbEntityTypes != null && dbEntityTypes.Any())
        {
            foreach (var dbEntityType in dbEntityTypes)
            {
                var dbAccessorType = typeof(DbAccessor<>).MakeGenericType(dbEntityType);
                _container
                    .RegisterType(dbAccessorType)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }
        }
    }
}