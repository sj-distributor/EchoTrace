namespace EchoTrace.Infrastructure.DataPersistence.DataEntityBases;

public interface IHasCreator : IHasCreatedOn
{
    Guid? CreatedBy { get; set; }
}