using System.Globalization;
using EchoTrace.Engines.Bases;
using EchoTrace.Infrastructure.CorsFunction;
using EchoTrace.Infrastructure.Hangfire;
using EchoTrace.Infrastructure.JwtFunction;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Engines.ConventionEngines;

/// <summary>
///     一些常规的配置
/// </summary>
public class ConfigureConvention : IBuilderEngine
{
    private readonly IServiceCollection services;

    public ConfigureConvention(IServiceCollection services)
    {
        this.services = services;
    }

    public void Run()
    {
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new Rfc1123DateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new Rfc1123DateTimeOffsetConverter());
        });
        services.AddRouting(e => { e.LowercaseUrls = true; });
        services.AddEndpointsApiExplorer();
        services.AddCorsFunction();
        services.AddJwtFunction();
        services.AddMemoryCache();
        services.AddHangfire();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSwaggerGen(options => { options.DescribeAllParametersInCamelCase(); });
    }
}

public class UseConvention : IAppEngine
{
    private readonly WebApplication app;

    public UseConvention(WebApplication app)
    {
        this.app = app;
    }

    public void Run()
    {
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHangfireDashboard();
        ValidatorOptions.Global.LanguageManager.Enabled = true;
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("zh-CN");
    }
}