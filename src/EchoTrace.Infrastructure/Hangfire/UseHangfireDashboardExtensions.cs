using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Infrastructure.Hangfire;

public static class UseHangfireDashboardExtensions
{
    public static WebApplication UseHangfireDashboard(this WebApplication app)
    {
        var lifetimeScope = app.Services.GetAutofacRoot();
        var hangfireSettings = lifetimeScope.Resolve<HangfireSettings>();
        var monitoringProjectDbSet = lifetimeScope.Resolve<DbAccessor<MonitoringProject>>();
        var monitoringProjectApiDbSet = lifetimeScope.Resolve<DbAccessor<MonitoringProjectApi>>();
        var monitoringProjectApiRequestHeaderInfoDbSet = lifetimeScope.Resolve<DbAccessor<MonitoringProjectApiRequestHeaderInfo>>();
        var monitoringProjectApiQueryParameterDbSet = lifetimeScope.Resolve<DbAccessor<MonitoringProjectApiQueryParameter>>();

        if (Convert.ToBoolean(hangfireSettings.EnableHangfireDashboard))
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "JobHub-Jobs",
                Authorization = new[]
                {
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = hangfireSettings.UserName,
                        Pass = hangfireSettings.Password
                    }
                },
                IsReadOnlyFunc = _ => false
            });
        }

        var allMonitoringProjects = monitoringProjectDbSet.DbSet.AsNoTracking().ToList();
        var allMonitoringProjectApis = monitoringProjectApiDbSet.DbSet.AsNoTracking().Where(x=> !x.IsDeactivate).ToList();
        var allMonitoringProjectApiIds = allMonitoringProjectApis.Select(x => x.Id).ToList();
        var allMonitoringProjectApiRequestHeaderInfos = monitoringProjectApiRequestHeaderInfoDbSet.DbSet.AsNoTracking()
            .Where(x => allMonitoringProjectApiIds.Contains(x.MonitoringProjectApiId)).ToList();
        var allMonitoringProjectApiQueryParameter = monitoringProjectApiQueryParameterDbSet.DbSet.AsNoTracking()
            .Where(x => allMonitoringProjectApiIds.Contains(x.MonitoringProjectApiId)).ToList();
        allMonitoringProjects.ForEach(x =>
        {
            var currentProjectJobs = allMonitoringProjectApis.Where(q => q.MonitoringProjectId == x.Id).ToList();
            currentProjectJobs.ForEach(y =>
            {
                var currentProjectJobRequestHeaderInfoList =
                    allMonitoringProjectApiRequestHeaderInfos.Where(q => q.MonitoringProjectApiId == y.Id).ToList();
                var currentProjectJobQueryParameterList =
                    allMonitoringProjectApiQueryParameter.Where(q => q.MonitoringProjectApiId == y.Id).ToList();
                var jobId = x.Name + "_" + y.ApiName;

                var recurringJobInfo = new RecurringJobInfo
                {
                    ApiId = y.Id,
                    Url = x.BaseUrl + y.ApiUrl,
                    MonitoringProjectName = x.Name,
                    BodyJson = y.BodyJson,
                    HttpRequestMethod = y.HttpRequestMethod,
                    JobId = jobId,
                    ExpectationStatusCode = y.ExpectationCode,
                    MonitoringProjectApiRequestHeaderInfos = currentProjectJobRequestHeaderInfoList,
                    MonitoringProjectApiQueryParameterList = currentProjectJobQueryParameterList
                };
                RecurringJob.AddOrUpdate<IHangfireRegisterJobHelper>(jobId,
                    service => service.RunRecurringJob(recurringJobInfo, null), () => y.MonitorInterval.ToCronExpression());
            });
        });

        return app;
    }
}