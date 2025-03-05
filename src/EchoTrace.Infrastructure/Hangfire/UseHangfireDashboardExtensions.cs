using Autofac;
using Autofac.Extensions.DependencyInjection;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
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
                RecurringJob.AddOrUpdate<IHangfireRegisterJobHelper>(jobId,
                    service => service.RunRecurringJob(y.Id, x.BaseUrl + y.ApiUrl,
                        x.Name, y.HttpRequestMethod, null, jobId, y.ExpectationCode,currentProjectJobRequestHeaderInfoList, currentProjectJobQueryParameterList, y.BodyJson), () => y.CronExpression);
            });
        });

        return app;
    }
}