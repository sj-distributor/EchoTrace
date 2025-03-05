using System.Net;
using System.Text;
using System.Text.Json;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using Hangfire.Server;
using Hangfire.Tags;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace EchoTrace.Infrastructure.Hangfire;

public interface IHangfireRegisterJobHelper
{
    Task RunRecurringJob(RecurringJobInfo recurringJobInfo, PerformContext context);
}

public class HangfireRegisterJobHelper(
    IMemoryCache memoryCache,
    HangfireSettings hangfireSettings,
    IHttpClientFactory httpClientFactory,
    ApplicationDbContext dbContext) : IHangfireRegisterJobHelper
{
    private const string JobCache = nameof(JobCache);

    private const string ApiError = nameof(ApiError);

    public async Task RunRecurringJob(
        RecurringJobInfo recurringJobInfo, PerformContext context)
    {
        var cacheKey = JobCache + "_" + recurringJobInfo.ApiId;
        Log.Information("开始执行定时任务，项目作业ID：{ProjectJobId}， 请求Url：{Url}， 请求方式：{HttpRequestMethod}"
            , recurringJobInfo.ApiId, recurringJobInfo.Url, recurringJobInfo.HttpRequestMethod.ToString());

        var jobCacheInfo = memoryCache.Get<JobCacheInfo>(cacheKey);
        if (jobCacheInfo != null)
        {
            return;
        }

        try
        {
            jobCacheInfo = new JobCacheInfo
            {
                ProjectJobId = recurringJobInfo.ApiId,
                Url = recurringJobInfo.Url
            };
            memoryCache.Set(cacheKey, jobCacheInfo, TimeSpan.FromMinutes(hangfireSettings.JobMemoryCacheTimeout));

            int failCount = 0;
            for (int i = 0; i < 10; i++)
            {
                var result = await SendHttpRequestWithApiKeyAsync(recurringJobInfo);
                if (result.StatusCode != recurringJobInfo.ExpectationStatusCode)
                {
                    failCount++;
                }
            }

            var healthLevel = HealthLevel.Health;
            if (failCount > 0)
            {
                var failureRate = failCount / 10.0 * 100;

                healthLevel = failureRate switch
                {
                    <= 10 => HealthLevel.Health,
                    > 10 and <= 50 => HealthLevel.Warn,
                    _ => HealthLevel.Dangerous
                };
            }

            var apiLog = new MonitoringProjectApiLog
            {
                MonitoringProjectApiId = recurringJobInfo.ApiId,
                HealthLevel = healthLevel
            };
            
            await dbContext.Set<MonitoringProjectApiLog>().AddAsync(apiLog);
        }
        catch (Exception e)
        {
            var apiLog = new MonitoringProjectApiLog
            {
                MonitoringProjectApiId = recurringJobInfo.ApiId,
                HealthLevel = HealthLevel.Dangerous
            };
            await dbContext.Set<MonitoringProjectApiLog>().AddAsync(apiLog);
            Log.Error(e, "执行定时任务失败，Job作业ID：{JobId}， 请求Url：{Url}， 请求方式：{HttpRequestMethod}",
                recurringJobInfo.JobId, recurringJobInfo.Url, recurringJobInfo.HttpRequestMethod.ToString());
            context.AddTags(recurringJobInfo.MonitoringProjectName + ApiError);
            throw;
        }
        finally
        {
            memoryCache.Remove(cacheKey);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task<HttpResponseMessage> SendHttpRequestWithApiKeyAsync(RecurringJobInfo recurringJobInfo)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(5);
        if (recurringJobInfo.MonitoringProjectApiRequestHeaderInfos != null)
        {
            recurringJobInfo.MonitoringProjectApiRequestHeaderInfos.ForEach(x =>
            {
                client.DefaultRequestHeaders.Add(x.RequestHeaderKey, x.RequestHeaderValue);
            });
        }

        object? contentObject = null;
        if (recurringJobInfo.BodyJson != null)
        {
            var bodyData = JsonSerializer.Deserialize<Dictionary<string, object>>(recurringJobInfo.BodyJson);
            contentObject = new Dictionary<string, object>(bodyData!);
        }

        var content = new StringContent(JsonSerializer.Serialize(contentObject), Encoding.UTF8, "application/json");
        if (recurringJobInfo.MonitoringProjectApiQueryParameterList != null && recurringJobInfo.MonitoringProjectApiQueryParameterList.Count > 0)
        {
            var queryString = string.Join("&",
                recurringJobInfo.MonitoringProjectApiQueryParameterList.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.ParameterName.ToLower())}={Uri.EscapeDataString(kvp.ParameterValue)}"));
            recurringJobInfo.Url += $"?{queryString}";
        }

        HttpResponseMessage result;
        switch (recurringJobInfo.HttpRequestMethod)
        {
            case HttpRequestMethod.Post:
                result = await client.PostAsync(recurringJobInfo.Url, content);
                break;
            case HttpRequestMethod.Patch:
                result = await client.PatchAsync(recurringJobInfo.Url, content);
                break;
            case HttpRequestMethod.Put:
                result = await client.PutAsync(recurringJobInfo.Url, content);
                break;
            case HttpRequestMethod.Delete:
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, recurringJobInfo.Url)
                {
                    Content = content
                };
                result = await client.SendAsync(requestMessage);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        return result;
    }

    private class JobCacheInfo
    {
        public Guid ProjectJobId { get; set; }

        public string Url { get; set; }

        public string ApiKey { get; set; }
    }
}

public class RecurringJobInfo
{
    public Guid ApiId { get; set; }
    
    public string Url { get; set; }

    public string MonitoringProjectName { get; set; }

    public string? BodyJson { get; set; } = null;

    public HttpRequestMethod HttpRequestMethod { get; set; }

    public string JobId { get; set; }

    public HttpStatusCode ExpectationStatusCode { get; set; }
    
    public List<MonitoringProjectApiRequestHeaderInfo> MonitoringProjectApiRequestHeaderInfos { get; set; }
    
    public List<MonitoringProjectApiQueryParameter> MonitoringProjectApiQueryParameterList { get; set; }
}