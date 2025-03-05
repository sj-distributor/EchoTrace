using System.ComponentModel;
using System.Net;
using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;

public class MonitoringProjectApi : IEfEntity<MonitoringProjectApi>, IHasKey<Guid>, IHasCreatedOn
{
    public MonitoringProjectApi()
    {
        this.InitPropertyValues();
    }
    
    public Guid Id { get; set; }
    
    public Guid MonitoringProjectId { get; set; }
    
    public string ApiName { get; set; }
    
    public string ApiUrl { get; set; }

    public string? BodyJson { get; set; }
    
    public HttpRequestMethod HttpRequestMethod { get; set; }

    public bool IsDeactivate { get; set; }

    public string CronExpression { get; set; }

    [Description("期望返回的状态码")]
    public HttpStatusCode ExpectationCode { get; set; }

    public DateTime CreatedOn { get; set; }
    
    public static void ConfigureEntityMapping(EntityTypeBuilder<MonitoringProjectApi> builder, IRelationalTypeMappingSource mappingSource)
    {
        builder.AutoConfigure(mappingSource);
        builder.ToTable(x => x.HasComment("监控项目接口"));
        builder.HasIndex(x => x.MonitoringProjectId);

        builder.Property(x => x.ApiUrl).HasMaxLength(1000).IsRequired();
    }
}