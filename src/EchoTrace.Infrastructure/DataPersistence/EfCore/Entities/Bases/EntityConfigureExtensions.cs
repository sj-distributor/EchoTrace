using System.Reflection;
using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;

public static class EntityConfigureExtensions
{
    public static void AutoConfigure<T>(this EntityTypeBuilder<T> builder,
        IRelationalTypeMappingSource mappingSource)
        where T : class, IEntity
    {
        var entityType = typeof(T);
        builder.ToTable(entityType.Name.Underscore());
        var entityProperties = GetEntityProperties(entityType);
        var propMethodInfo = typeof(EntityTypeBuilder<T>).GetMethod(nameof(EntityTypeBuilder<T>.Property), 0,
            [typeof(Type), typeof(string)])!;
        var idName = nameof(IHasKey<Guid>.Id);
        var interfaces = entityType.GetInterfaces();
        if (interfaces.Any(e => e == typeof(ICanSoftDelete)))
        {
            builder.HasQueryFilter(e => !((ICanSoftDelete)e).IsDeleted);
        }

        if (interfaces.Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IHasKey<>)))
        {
            builder.HasKey(idName);
        }

        foreach (var entityProperty in entityProperties)
        {
            var storeType = GetMySqlStoreType(mappingSource, entityProperty.PropertyType);
            if (!string.IsNullOrWhiteSpace(storeType))
            {
                if (propMethodInfo.Invoke(builder, [entityProperty.PropertyType, entityProperty.Name]) is
                    PropertyBuilder propBuilder)
                {
                    propBuilder.HasColumnName(entityProperty.Name.Underscore());
                    propBuilder.HasColumnType(storeType);
                    if (entityProperty.PropertyType.IsGenericType &&
                        entityProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var isRequiredType = typeof(PropertyBuilder).GetMethod(nameof(PropertyBuilder.IsRequired),
                            [typeof(bool)]);
                        isRequiredType?.Invoke(propBuilder, [false]);
                    }

                    if (entityProperty.PropertyType == typeof(string))
                    {
                        propBuilder.HasColumnType("varchar");
                        var hasMaxLengthType = typeof(PropertyBuilder).GetMethod(nameof(PropertyBuilder.HasMaxLength),
                            [typeof(int)]);
                        hasMaxLengthType?.Invoke(propBuilder, [100]);
                    }
                    else if (entityProperty.PropertyType == typeof(Guid))
                    {
                        var hasMaxLengthType = typeof(PropertyBuilder).GetMethod(nameof(PropertyBuilder.HasMaxLength),
                            [typeof(int)]);
                        hasMaxLengthType?.Invoke(propBuilder, [36]);
                    }
                }
            }
        }
    }


    private static PropertyInfo[] GetEntityProperties(Type? entityType)
    {
        var propertyInfos = new List<PropertyInfo>();
        if (entityType != null)
        {
            var entityTypeProperties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                                BindingFlags.GetProperty | BindingFlags.SetProperty |
                                                                BindingFlags.DeclaredOnly);
            propertyInfos.AddRange(entityTypeProperties);
            var baseProperties = GetEntityProperties(entityType.BaseType);
            foreach (var baseProperty in baseProperties)
            {
                if (!propertyInfos.Contains(baseProperty))
                {
                    propertyInfos.Add(baseProperty);
                }
            }
        }

        return propertyInfos.ToArray();
    }

    private static string? GetMySqlStoreType(IRelationalTypeMappingSource mappingSource, Type type)
    {
        var mapping = mappingSource.FindMapping(type);
        return mapping?.StoreType;
    }
}