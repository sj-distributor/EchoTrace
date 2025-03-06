using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;

public class MonitoringProjectApiRequestHeaderInfo : IEfEntity<MonitoringProjectApiRequestHeaderInfo>, IHasKey<Guid>, IHasCreatedOn
{
    public MonitoringProjectApiRequestHeaderInfo()
    {
        this.InitPropertyValues();
    }
    
    public Guid Id { get; set; }
    
    public Guid MonitoringProjectApiId { get; set; }
    
    public string RequestHeaderKey { get; set; }
    
    public string RequestHeaderValue { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public static void ConfigureEntityMapping(EntityTypeBuilder<MonitoringProjectApiRequestHeaderInfo> builder, IRelationalTypeMappingSource mappingSource)
    {
        builder.AutoConfigure(mappingSource);
        builder.ToTable(x => x.HasComment("Api请求头信息"));
        builder.HasIndex(x => x.MonitoringProjectApiId);
    }
}