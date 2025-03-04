using MongoDB.Bson;

namespace EchoTrace.Infrastructure.DataPersistence.DataEntityBases;

public static class InitEntityExtensions
{
    public static void InitPropertyValues<T>(this T entity) where T : IEntity
    {
        if (entity is IHasKey<Guid> hasKey)
        {
            hasKey.Id = Guid.NewGuid();
        }

        if (entity is IHasCreatedOn hasCreatedDate)
        {
            hasCreatedDate.CreatedOn = DateTime.UtcNow;
        }

        if (entity is ICanSoftDelete canSoftDelete)
        {
            canSoftDelete.IsDeleted = false;
        }

        if (entity is IHasKey<ObjectId> hasObjectId)
        {
            hasObjectId.Id = ObjectId.GenerateNewId();
        }
    }
}