namespace EchoTrace.Realization.Bases;

public static class EntityExtensions
{
    public static T Faker<T>(this T entity, Action<T>? action = null) where T : class
    {
        var nweEntity = BusinessFaker<T>.Create();
        action?.Invoke(nweEntity);
        return nweEntity;
    }

    public static List<T> Faker<T>(this T entity, int number, Action<T, int>? action = null)
        where T : class
    {
        var entities = BusinessFaker<T>.Create(number);
        for (int i = 0; i < entities.Count; i++)
        {
            action?.Invoke(entities[i], i);
        }

        return entities;
    }

    public static List<T> Faker<T>(this T entity, int number, Action<T> action)
        where T : class
    {
        return entity.Faker(number, (obj, _) => action.Invoke(obj));
    }
}