using System.Reflection;
using System.Reflection.Emit;

namespace EchoTrace.Realization.Bases;

[AsType(LifetimeEnum.SingleInstance)]
public class AgileMapper : IAgileMapper
{
    public void Map<TSource, TTarget>(TSource source, TTarget? target,
        Action<AgileMapperConfiguration<TSource, TTarget>>? configure = null)
        where TSource : class where TTarget : class, new()
    {
        var targetType = typeof(TTarget);
        var sourceType = typeof(TSource);
        target ??= new();
        var configuration = new AgileMapperConfiguration<TSource, TTarget>();
        configure?.Invoke(configuration);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var mapFromProfile = new MapFromProfile<TSource, TTarget>
        {
            Source = source,
            Target = target
        };

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var targetProperty in targetProperties)
        {
            if (targetProperty is { CanWrite: true, SetMethod: not null })
            {
                var @event = configuration.PropertyEvents.FirstOrDefault(e =>
                    e.PropertyInfo.PropertyType == targetProperty.PropertyType &&
                    e.PropertyInfo.Name == targetProperty.Name);
                if (@event is not null)
                {
                    if (@event.Functions is { Count: > 0 })
                    {
                        foreach (var func in @event.Functions)
                        {
                            var tempProfile = new MapFromProfile<TSource, TTarget>
                            {
                                Source = source,
                                Target = new()
                            };
                            func.DynamicInvoke(tempProfile);
                            if (!tempProfile.IsIgnore)
                            {
                                var value = func.DynamicInvoke(mapFromProfile);
                                var propertyMethod =
                                    SetPropertyMethod<TTarget>(targetProperty);
                                propertyMethod.DynamicInvoke(target, value);
                            }
                        }
                    }

                    if (@event.Actions is { Count: > 0 })
                    {
                        foreach (var action in @event.Actions)
                        {
                            var tempProfile = new MapFromProfile<TSource, TTarget>
                            {
                                Source = source,
                                Target = new()
                            };
                            action.DynamicInvoke(tempProfile);
                            if (!tempProfile.IsIgnore)
                            {
                                action.DynamicInvoke(mapFromProfile);
                            }
                        }
                    }
                }
                else
                {
                    var sourceProperty = sourceProperties.FirstOrDefault(e =>
                        e.PropertyType == targetProperty.PropertyType && e.Name == targetProperty.Name);
                    if (sourceProperty is { CanRead: true, GetMethod: not null })
                    {
                        var copyPropertyMethod = CopyPropertyMethod<TSource, TTarget>(sourceProperty, targetProperty);
                        copyPropertyMethod?.Invoke(source, target);
                    }
                }
            }
        }
    }

    private static Action<TSource, TTarget> CopyPropertyMethod<TSource, TTarget>(PropertyInfo sourcePropertyInfo,
        PropertyInfo targetPropertyInfo)
    {
        var copyPropertiesIlMethod =
            new DynamicMethod("IlCopyPropertyMethod", typeof(void), new[] { typeof(TSource), typeof(TTarget) });
        copyPropertiesIlMethod.DefineParameter(position: 0, ParameterAttributes.None, "source");
        copyPropertiesIlMethod.DefineParameter(position: 1, ParameterAttributes.None, "target");
        var iLGenerator = copyPropertiesIlMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.Emit(OpCodes.Call, sourcePropertyInfo.GetMethod!);
        iLGenerator.Emit(OpCodes.Call, targetPropertyInfo.SetMethod!);
        iLGenerator.Emit(OpCodes.Ret);
        var copyAction =
            (Action<TSource, TTarget>)copyPropertiesIlMethod.CreateDelegate(typeof(Action<TSource, TTarget>));
        return copyAction;
    }

    private static Delegate SetPropertyMethod<TTarget>(PropertyInfo targetPropertyInfo)
    {
        var targetType = typeof(TTarget);
        var copyPropertiesIlMethod =
            new DynamicMethod("IlSetPropertyMethod", typeof(void),
                new[] { targetType, targetPropertyInfo.PropertyType });
        copyPropertiesIlMethod.DefineParameter(position: 0, ParameterAttributes.None, "target");
        copyPropertiesIlMethod.DefineParameter(position: 1, ParameterAttributes.None, "value");
        var iLGenerator = copyPropertiesIlMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Call, targetPropertyInfo.SetMethod!);
        iLGenerator.Emit(OpCodes.Ret);
        var setPropertyMethod =
            copyPropertiesIlMethod.CreateDelegate(
                typeof(Action<,>).MakeGenericType(targetType, targetPropertyInfo.PropertyType));
        return setPropertyMethod;
    }

    public TTarget Map<TSource, TTarget>(TSource source,
        Action<AgileMapperConfiguration<TSource, TTarget>>? configure = null)
        where TSource : class where TTarget : class, new()
    {
        TTarget target = new TTarget();
        Map(source, target, configure);
        return target;
    }
}