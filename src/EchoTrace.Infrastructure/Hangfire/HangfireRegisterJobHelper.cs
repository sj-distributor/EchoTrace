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
    Task RunRecurringJob(
        Guid apiId, string url, string monitoringProjectName, HttpRequestMethod httpRequestMethod,
        PerformContext context, string jobId, HttpStatusCode statusCode,
        List<MonitoringProjectApiRequestHeaderInfo>? monitoringProjectApiRequestHeaderInfos = null,
        List<MonitoringProjectApiQueryParameter>? monitoringProjectApiQueryParameterList = null,
        string? bodyJson = null);
}

public class HangfireRegisterJobHelper(
    IMemoryCache memoryCache,
    HangfireSettings hangfireSettings,
    IHttpClientFactory httpClientFactory,
    DbAccessor<MonitoringProjectApiLog> monitoringProjectApiLogDbSet) : IHangfireRegisterJobHelper
{
    private const string JobCache = nameof(JobCache);

    private const string ApiError = nameof(ApiError);

    public async Task RunRecurringJob(
        Guid apiId, string url, string monitoringProjectName, HttpRequestMethod httpRequestMethod,
        PerformContext context, string jobId, HttpStatusCode statusCode,
        List<MonitoringProjectApiRequestHeaderInfo>? monitoringProjectApiRequestHeaderInfos = null,
        List<MonitoringProjectApiQueryParameter>? monitoringProjectApiQueryParameterList = null,
        string? bodyJson = null)
    {
        var cacheKey = JobCache + "_" + apiId;
        Log.Information("开始执行定时任务，项目作业ID：{ProjectJobId}， 请求Url：{Url}， 请求方式：{HttpRequestMethod}"
            , apiId, url, httpRequestMethod.ToString());

        var jobCacheInfo = memoryCache.Get<JobCacheInfo>(cacheKey);
        if (jobCacheInfo != null)
        {
            return;
        }

        try
        {
            jobCacheInfo = new JobCacheInfo
            {
                ProjectJobId = apiId,
                Url = url
            };
            memoryCache.Set(cacheKey, jobCacheInfo, TimeSpan.FromMinutes(hangfireSettings.JobMemoryCacheTimeout));

            int failCount = 0;
            for (int i = 0; i < 10; i++)
            {
                var result = await SendHttpRequestWithApiKeyAsync(url, httpRequestMethod,
                    monitoringProjectApiRequestHeaderInfos, monitoringProjectApiQueryParameterList, bodyJson);
                if (result.StatusCode != statusCode)
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
                MonitoringProjectApiId = apiId,
                HealthLevel = healthLevel
            };
            
            await monitoringProjectApiLogDbSet.DbSet.AddAsync(apiLog);
        }
        catch (Exception e)
        {
            Log.Error(e, "执行定时任务失败，Job作业ID：{JobId}， 请求Url：{Url}， 请求方式：{HttpRequestMethod}",
                jobId, url, httpRequestMethod.ToString());
            context.AddTags(monitoringProjectName + ApiError);
            throw;
        }
        finally
        {
            memoryCache.Remove(cacheKey);
        }
    }

    private async Task<HttpResponseMessage> SendHttpRequestWithApiKeyAsync(string url,
        HttpRequestMethod httpRequestMethod,
        List<MonitoringProjectApiRequestHeaderInfo>? monitoringProjectApiRequestHeaderInfos = null,
        List<MonitoringProjectApiQueryParameter>? monitoringProjectApiQueryParameterList = null,
        string? bodyJson = null)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(5);
        if (monitoringProjectApiRequestHeaderInfos != null)
        {
            monitoringProjectApiRequestHeaderInfos.ForEach(x =>
            {
                client.DefaultRequestHeaders.Add(x.RequestHeaderKey, x.RequestHeaderValue);
            });
        }

        object? contentObject = null;
        if (bodyJson != null)
        {
            var bodyData = JsonSerializer.Deserialize<Dictionary<string, object>>(bodyJson);
            contentObject = new Dictionary<string, object>(bodyData!);
        }

        var content = new StringContent(JsonSerializer.Serialize(contentObject), Encoding.UTF8, "application/json");
        if (monitoringProjectApiQueryParameterList != null && monitoringProjectApiQueryParameterList.Count > 0)
        {
            var queryString = string.Join("&",
                monitoringProjectApiQueryParameterList.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.ParameterName.ToLower())}={Uri.EscapeDataString(kvp.ParameterValue)}"));
            url += $"?{queryString}";
        }

        HttpResponseMessage result;
        switch (httpRequestMethod)
        {
            case HttpRequestMethod.Post:
                result = await client.PostAsync(url, content);
                break;
            case HttpRequestMethod.Patch:
                result = await client.PatchAsync(url, content);
                break;
            case HttpRequestMethod.Put:
                result = await client.PutAsync(url, content);
                break;
            case HttpRequestMethod.Delete:
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url)
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