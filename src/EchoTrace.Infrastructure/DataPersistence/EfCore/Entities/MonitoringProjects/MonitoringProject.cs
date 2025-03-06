using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;

public class MonitoringProject : IEfEntity<MonitoringProject>, IHasKey<Guid>, IHasCreatedOn
{
    public MonitoringProject()
    {
        this.InitPropertyValues();
    }
    
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public DateTime CreatedOn { get; set; }
    
    public static void ConfigureEntityMapping(EntityTypeBuilder<MonitoringProject> builder, IRelationalTypeMappingSource mappingSource)
    {
        builder.AutoConfigure(mappingSource);
        builder.ToTable(x => x.HasComment("监控项目"));

        builder.Property(x => x.BaseUrl).HasMaxLength(1000).IsRequired();
    }
}