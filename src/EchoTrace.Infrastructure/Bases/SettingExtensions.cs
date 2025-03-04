using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Infrastructure.Bases;

public static class SettingExtensions
{
    public static IConfigurationRoot AddSettings(this IServiceCollection services, Action<SettingOptions> setOption)
    {
        var settingOptions = new SettingOptions();
        setOption.Invoke(settingOptions);
        services.AddSingleton(settingOptions);
        var iSettingType = typeof(ISetting);
        var infrastructureAssembly = iSettingType.Assembly;
        var settingTypes = infrastructureAssembly
            .ExportedTypes
            .Where(e => e.GetInterfaces().Contains(iSettingType) && e is { IsClass: true, IsAbstract: false })
            .ToArray();
        var basePath = AppContext.BaseDirectory;
        IConfigurationBuilder builder = new ConfigurationBuilder();
        foreach (var settingType in settingTypes)
        {
            var setting = Activator.CreateInstance(settingType)!;
            if (setting is IJsonFileSetting jsonFileSetting)
            {
                var path = Path.Combine(basePath, jsonFileSetting.JsonFilePath);
                if (!string.IsNullOrWhiteSpace(jsonFileSetting.JsonFilePath)
                    && File.Exists(path))
                {
                    builder.AddJsonFile(path, false);
                }
            }
            else if (setting is IStringSetting stringSetting)
            {
                services.AddSingleton(settingType, stringSetting);
            }
        }
        
        builder.AddUserSecrets(assembly: Assembly.GetExecutingAssembly(),
            optional: true,
            reloadOnChange: true);

        if (File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            builder.AddJsonFile(Path.Combine(basePath, "appsettings.json"), false);
        }

        var configuration = builder
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton(configuration);
        services.AddSingleton<IConfiguration>(configuration);
        foreach (var settingType in settingTypes)
        {
            var setting = Activator.CreateInstance(settingType)!;
            var settingName = settingType.Name;
            configuration.GetSection(settingName).Bind(setting);
            services.AddSingleton(settingType, setting);
        }

        return configuration;
    }
}