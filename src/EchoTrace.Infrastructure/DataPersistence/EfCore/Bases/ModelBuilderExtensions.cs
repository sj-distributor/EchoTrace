using System.Reflection;
using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Bases;

public static class ModelBuilderExtensions
{
    public static void LoadFromEntityConfigure(this ModelBuilder modelBuilder,
        IRelationalTypeMappingSource mappingSource)
    {
        var idbEntityType = typeof(IEfEntity<>);
        var idbEntityAssembly = idbEntityType.Assembly;
        var dbEntityTypes = idbEntityAssembly
            ?.ExportedTypes
            .Where(e => e.GetInterfaces()
                            .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == idbEntityType) && e.IsClass &&
                        !e.IsAbstract)
            .ToArray();
        if (dbEntityTypes is { Length: > 0 })
        {
            foreach (var dbEntityType in dbEntityTypes)
            {
                var entityBuilderAction = typeof(ModelBuilder).GetMethod(nameof(ModelBuilder.Entity),
                    BindingFlags.Public | BindingFlags.Instance, []);
                if (entityBuilderAction is null)
                {
                    throw new InvalidOperationException("Entity mapping failure");
                }

                var entityBuilderGenericAction = entityBuilderAction.MakeGenericMethod(dbEntityType);
                var builder = entityBuilderGenericAction.Invoke(modelBuilder, null);
                if (builder is null)
                {
                    throw new InvalidOperationException("Entity mapping failure");
                }

                var entityConfigureAction = dbEntityType.GetMethod(
                    nameof(IEfEntity<IEntity>.ConfigureEntityMapping), BindingFlags.Public | BindingFlags.Static,
                    [builder.GetType(), typeof(IRelationalTypeMappingSource)]);
                if (entityConfigureAction is null)
                {
                    throw new InvalidOperationException("Entity mapping failure");
                }

                entityConfigureAction.Invoke(null, [builder, mappingSource]);
            }
        }
    }
}