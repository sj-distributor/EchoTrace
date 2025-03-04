using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Engines.Bases;

internal class CustomAutofacServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
{
    public ContainerBuilder CreateBuilder(IServiceCollection services)
    {
        var builder = new ContainerBuilder();
        return builder;
    }

    public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
    {
        if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));
        var container = containerBuilder.Build();
        return new AutofacServiceProvider(container);
    }
}