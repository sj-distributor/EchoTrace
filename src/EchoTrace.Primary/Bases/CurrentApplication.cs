using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Primary.Bases;

public static class CurrentApplication
{
    public static void Build(ILifetimeScope serviceProvider)
    {
        if (IsBuilt)
        {
            throw new InvalidOperationException("Current application has been built,can not modify again");
        }

        ServiceProvider = serviceProvider;
        IsBuilt = true;
        CurrentRunId = Guid.NewGuid();
    }

    private static bool IsBuilt { get; set; }

    public static ILifetimeScope ServiceProvider { get; private set; }

    public static bool TryContextResolve<T>(out T? service) where T : class
    {
        if (ServiceProvider.TryResolve<IHttpContextAccessor>(out var httpContextAccessor))
        {
            if (httpContextAccessor is { HttpContext: not null })
            {
                var scope = httpContextAccessor.HttpContext.RequestServices.GetService<ILifetimeScope>();
                if (scope != null)
                {
                    if (scope.TryResolve(out service))
                    {
                        return true;
                    }
                }
            }
        }

        return ServiceProvider.TryResolve(out service);
    }

    public static bool TryContextResolve(Type type, out object? service)
    {
        if (ServiceProvider.TryResolve<IHttpContextAccessor>(out var httpContextAccessor))
        {
            if (httpContextAccessor is { HttpContext: not null })
            {
                var scope = httpContextAccessor.HttpContext.RequestServices.GetService<ILifetimeScope>();
                if (scope != null)
                {
                    if (scope.TryResolve(type, out service))
                    {
                        return true;
                    }
                }
            }
        }

        return ServiceProvider.TryResolve(type, out service);
    }

    public static Guid CurrentRunId { get; private set; }
}