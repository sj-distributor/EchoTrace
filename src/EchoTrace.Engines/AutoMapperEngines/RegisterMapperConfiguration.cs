using System.Reflection;
using EchoTrace.Engines.Bases;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Realization.Bases;
using Mediator.Net.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Engines.AutoMapperEngines;

public class RegisterMapperConfiguration : IBuilderEngine
{
    private readonly IServiceCollection _services;

    public RegisterMapperConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    public void Run()
    {
        var mapperTypes = GetAssemblyMapperTypes(typeof(IContract<IMessage>).Assembly, typeof(IRealization).Assembly);
        _services.AddAutoMapper(cfg =>
        {
            foreach (var mapperType in mapperTypes)
            {
                var sourceInterfaceTypes = mapperType.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMapFrom<>));

                foreach (var sourceInterfaceType in sourceInterfaceTypes)
                {
                    var sourceType = sourceInterfaceType.GenericTypeArguments.First();
                    cfg.CreateMap(sourceType, mapperType);
                    var methods = mapperType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.Name == nameof(IMapFrom<object>.ConfigureMapper));
                    foreach (var method in methods)
                    {
                        method.Invoke(Activator.CreateInstance(mapperType), [
                            cfg, null
                        ]);
                    }
                }
            }
        });
    }

    private List<Type> GetAssemblyMapperTypes(params Assembly[] assemblies)
    {
        var mapperTypes = new List<Type>();
        foreach (var assembly in assemblies)
        {
            mapperTypes.AddRange(
                assembly
                    .ExportedTypes
                    .Where(x => x is { IsClass: true, IsAbstract: false } && x.GetInterfaces()
                        .Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IMapFrom<>))));
        }

        return mapperTypes;
    }
}