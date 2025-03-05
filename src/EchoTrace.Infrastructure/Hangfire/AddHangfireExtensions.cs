using EchoTrace.Infrastructure.JwtFunction;
using Hangfire;
using Hangfire.Pro.Redis;
using Hangfire.Tags;
using Hangfire.Tags.Pro.Redis;
using JobHub.Infrastructure.Hangfire.HangfireStateHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace EchoTrace.Infrastructure.Hangfire;

public static class AddHangfireExtensions
{
    public static IServiceCollection AddHangfire(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var hangfireSettings = serviceProvider.GetRequiredService<HangfireSettings>();
        services.AddScoped<IPasswordHasher, PasswordHash>();
        services.AddScoped<IHangfireRegisterJobHelper, HangfireRegisterJobHelper>();
        services.AddScoped<IRobotSendLogService, RobotSendLogService>();

        var redisOptions = new RedisStorageOptions
        {
            Database = hangfireSettings.RedisDb,
            InvisibilityTimeout = TimeSpan.FromMinutes(20),
            Prefix = "Hangfire"
        };
        if (!string.IsNullOrWhiteSpace(hangfireSettings.RedisConnectionString))
        {
            services.AddHangfire(x =>
            {
                x.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
                x.SetDataCompatibilityLevel(CompatibilityLevel.Version_170) //此方法 只初次创建数据库使用即可
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseRedisStorage(hangfireSettings.RedisConnectionString, redisOptions)
                    .UseTagsWithRedis(new TagsOptions
                    {
                        TagsListStyle = TagsListStyle.Dropdown,
                        Clean = Clean.None
                    }, redisOptions);
            });
            
            services.AddHangfireServer(o =>
            {
                o.ServerName = "HangFireSyncDataTask";
                o.SchedulePollingInterval = TimeSpan.FromSeconds(3);
                o.WorkerCount = 5; //并行数
            });
            GlobalStateHandlers.Handlers.Add(new SucceededJobExpireHandler(hangfireSettings.SucceededJobExpireTime));
            GlobalStateHandlers.Handlers.Add(new FailedJobExpireHandler(hangfireSettings.FailedJobExpireTime));
        }

        return services;
    }
}