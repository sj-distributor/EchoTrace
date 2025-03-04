using EchoTrace.Engines.Bases;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace EchoTrace.Engines.SwaggerEngines;

public class UseSwagger : IAppEngine
{
    private readonly WebApplication app;

    public UseSwagger(WebApplication app)
    {
        this.app = app;
    }

    public void Run()
    {
        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                typeof(SwaggerApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                {
                    var info = f.GetCustomAttributes(typeof(SwaggerGroupInfoAttribute), false)
                        .OfType<SwaggerGroupInfoAttribute>().FirstOrDefault();
                    options.SwaggerEndpoint($"/swagger/{f.Name}/swagger.json",
                        (info != null ? info.Title : f.Name) + "-" + app.Environment.EnvironmentName);
                });
                options.SwaggerEndpoint("/swagger/Other/swagger.json", "其他" + "-" + app.Environment.EnvironmentName);
            });
        }
    }
}