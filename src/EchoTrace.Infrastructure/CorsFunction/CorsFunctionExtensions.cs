using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EchoTrace.Infrastructure.CorsFunction;

public static class CorsFunctionExtensions
{
    public static IServiceCollection AddCorsFunction(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var corSetting = serviceProvider.GetService<CorsSetting>();
                var environment = serviceProvider.GetService<IWebHostEnvironment>();
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
                if (environment != null && environment.IsProduction())
                {
                    policy.WithOrigins(corSetting?.Origins ?? [])
                        .AllowCredentials();
                }
                else
                {
                    if (corSetting is not { Origins.Length: > 0 })
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(corSetting.Origins);
                    }
                }
            });
        });
    }
}