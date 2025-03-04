namespace EchoTrace.Infrastructure.DataPersistence.DataEntityBases;

public interface ICanSoftDelete : IEntity
{
    public bool IsDeleted { get; set; }
}