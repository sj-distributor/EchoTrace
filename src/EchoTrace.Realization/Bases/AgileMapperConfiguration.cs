using System.Linq.Expressions;
using System.Reflection;

namespace EchoTrace.Realization.Bases;

public class AgileMapperConfiguration<TSource, TTarget>
{
    public AgileMapperConfiguration()
    {
        PropertyEvents = new HashSet<MapPropertyEvent>(new PropertyEventEqualityComparer());
    }

    public HashSet<MapPropertyEvent> PropertyEvents { get; }

    public AgileMapperConfiguration<TSource, TTarget> Profile<TProperty>(
        Expression<Func<MapToProfile<TTarget>, TProperty>> to,
        Func<MapFromProfile<TSource, TTarget>, TProperty> from
    )
    {
        if (to.Body is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            var newEvent = new MapPropertyEvent { PropertyInfo = propertyInfo };
            if (PropertyEvents.TryGetValue(newEvent, out var oldEvent))
            {
                oldEvent.Functions.Add(from);
            }
            else
            {
                newEvent.Functions.Add(from);
                PropertyEvents.Add(newEvent);
            }
        }

        return this;
    }

    public AgileMapperConfiguration<TSource, TTarget> Profile<TProperty>(
        Expression<Func<MapToProfile<TTarget>, TProperty>> to,
        TProperty value
    )
    {
        return Profile(to, from => from.Value(value));
    }

    public AgileMapperConfiguration<TSource, TTarget> Profile<TProperty>(
        Expression<Func<MapToProfile<TTarget>, TProperty>> to,
        Action<MapFromProfile<TSource, TTarget>> from
    )
    {
        if (to.Body is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            var newEvent = new MapPropertyEvent { PropertyInfo = propertyInfo };
            if (PropertyEvents.TryGetValue(newEvent, out var oldEvent))
            {
                oldEvent.Actions.Add(from);
            }
            else
            {
                newEvent.Actions.Add(from);
                PropertyEvents.Add(newEvent);
            }
        }

        return this;
    }
}