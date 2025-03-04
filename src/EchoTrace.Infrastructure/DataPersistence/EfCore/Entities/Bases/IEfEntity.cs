using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;

/// <summary>
///     如果一个实体后续会使用EfCore进行操作，请实现该接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEfEntity<T> : IEntity where T : class, IEntity
{
    public static abstract void ConfigureEntityMapping(EntityTypeBuilder<T> builder,
        IRelationalTypeMappingSource mappingSource);
}