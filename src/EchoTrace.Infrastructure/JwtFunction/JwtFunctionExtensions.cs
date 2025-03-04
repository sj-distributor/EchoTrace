using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EchoTrace.Infrastructure.JwtFunction;

public static class JwtFunctionExtensions
{
    public static IServiceCollection AddJwtFunction(this IServiceCollection services)
    {
        services
            .AddSingleton<JwtBearerOptions>(provider =>
            {
                var setting = provider.GetRequiredService<JwtSetting>();
                JwtBearerOptions options = new JwtBearerOptions
                {
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = setting.Issuer,
                        ValidTypes = [TokenTypeEnum.AccessToken.ToString()],
                        ValidAudience = setting.Audience,
                        RequireExpirationTime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SignKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    }
                };
                return options;
            })
            .AddSingleton<JwtService>()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
            });
        return services;
    }
}