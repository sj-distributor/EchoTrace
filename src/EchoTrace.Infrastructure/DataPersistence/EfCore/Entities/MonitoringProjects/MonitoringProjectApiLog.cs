using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;

public class MonitoringProjectApiLog : IEfEntity<MonitoringProjectApiLog>, IHasKey<Guid>, IHasCreatedOn
{
    public MonitoringProjectApiLog()
    {
        this.InitPropertyValues();
    }
    
    public Guid Id { get; set; }
    
    public Guid MonitoringProjectApiId { get; set; }

    public HealthLevel HealthLevel { get; set; }

    public DateTime CreatedOn { get; set; }
    
    public static void ConfigureEntityMapping(EntityTypeBuilder<MonitoringProjectApiLog> builder, IRelationalTypeMappingSource mappingSource)
    {
        builder.AutoConfigure(mappingSource);
        builder.ToTable(x => x.HasComment("监控项目接口日志"));
        builder.HasIndex(x => x.MonitoringProjectApiId);
    }
}