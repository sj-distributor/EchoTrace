using System.Net;
using System.Net.Http.Json;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using Microsoft.Extensions.Hosting;

namespace EchoTrace.Infrastructure.Hangfire;

public interface IRobotSendLogService
{
    Task SendJobFailLogAsync(string jobId, string url, HttpStatusCode errorStatusCode, HttpRequestMethod httpRequestMethod,
        List<MonitoringProjectApiQueryParameter>? monitoringProjectApiQueryParameter = null,
        string? bodyJson = null, CancellationToken cancellationToken = default);
}

public class RobotSendLogService(IHttpClientFactory httpClientFactory, HangfireSettings hangfireSettings, IHostEnvironment hostEnvironment) : IRobotSendLogService
{
    public async Task SendJobFailLogAsync(string jobId, string url, HttpStatusCode errorStatusCode, HttpRequestMethod httpRequestMethod,
        List<MonitoringProjectApiQueryParameter>? monitoringProjectApiQueryParameter = null,
        string? bodyJson = null, CancellationToken cancellationToken = default)
    {
        var queryString = "";
        var environment = hostEnvironment.IsProduction() ? "Prd: " : "Test: ";
        if (monitoringProjectApiQueryParameter != null && monitoringProjectApiQueryParameter.Count > 0)
        {
            queryString = string.Join("&", monitoringProjectApiQueryParameter.Select(kvp => $"{Uri.EscapeDataString(kvp.ParameterName.ToLower())}={Uri.EscapeDataString(kvp.ParameterValue)}"));
        }
        try
        {
            using var client = httpClientFactory.CreateClient();
            await client.PostAsJsonAsync(hangfireSettings.RobotUrl,
                new
                {
                    msgtype = "text",
                    text = new
                    {
                        content =
                            $"{environment} JobName:{jobId} Running failed\n" +
                            $" ApiUrl:{url} \n" +
                            $"QueryString:{queryString} \n" +
                            $"BodyJson:{bodyJson} \n" +
                            $"HttpRequestMethod:{httpRequestMethod.ToString()}\n" +
                            $" ErrorStatusCode:{errorStatusCode}"
                    }
                }, cancellationToken);
        }
        catch
        {
            // ignored
        }
    }
}