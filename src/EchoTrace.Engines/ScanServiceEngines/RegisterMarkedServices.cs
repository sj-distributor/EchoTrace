using System.Reflection;
using EchoTrace.Engines.Bases;
using EchoTrace.Realization.Bases;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Engines.ScanServiceEngines;

public class RegisterMarkedServices : IBuilderEngine
{
    private readonly IServiceCollection services;

    public RegisterMarkedServices(IServiceCollection services)
    {
        this.services = services;
    }

    public void Run()
    {
        var iRealizationType = typeof(IRealization);
        var types = iRealizationType
            .Assembly
            .ExportedTypes
            .Where(e => e.GetCustomAttribute<AsTypeAttribute>() != null)
            .ToArray();

        foreach (var implementationType in types)
        {
            var asTypeFlag = implementationType.GetCustomAttribute<AsTypeAttribute>()!;
            if (asTypeFlag is { Types: { Length: > 0 } })
            {
                foreach (var baseType in asTypeFlag.Types)
                    AddService(services, asTypeFlag.Lifetime, baseType, implementationType);
            }
            else
            {
                var baseTypes = implementationType.GetInterfaces();
                if (baseTypes is { Length: > 0 })
                {
                    foreach (var baseType in baseTypes)
                    {
                        AddService(services, asTypeFlag.Lifetime, baseType, implementationType);
                    }
                }

                if (implementationType.BaseType != null)
                {
                    AddService(services, asTypeFlag.Lifetime, implementationType.BaseType, implementationType);
                }

                AddService(services, asTypeFlag.Lifetime, implementationType, implementationType);
            }
        }
    }

    private static void AddService(IServiceCollection services, LifetimeEnum lifetime, Type baseType,
        Type implementationType)
    {
        switch (lifetime)
        {
            case LifetimeEnum.SingleInstance:
                services.AddSingleton(baseType, implementationType);
                break;
            case LifetimeEnum.Transient:
                services.AddTransient(baseType, implementationType);
                break;
            case LifetimeEnum.Scope:
                services.AddScoped(baseType, implementationType);
                break;
        }
    }
}