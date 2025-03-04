namespace EchoTrace.Infrastructure.DataPersistence.DataEntityBases;

/// <summary>
///     扩展表请基于本接口
/// </summary>
public interface IExtendedEntity<T> : IEntity where T : IEntity
{
    Guid MainId { get; set; }
}