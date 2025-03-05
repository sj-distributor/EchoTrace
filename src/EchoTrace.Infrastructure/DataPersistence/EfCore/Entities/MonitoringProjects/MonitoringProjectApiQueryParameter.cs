using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;

public class MonitoringProjectApiQueryParameter : IEfEntity<MonitoringProjectApiQueryParameter>, IHasKey<Guid>, IHasCreatedOn
{
    public Guid Id { get; set; }

    public Guid MonitoringProjectApiId { get; set; }
    
    public string ParameterName { get; set; }
    
    public string ParameterValue { get; set; }

    public DateTime CreatedOn { get; set; }
    
    public static void ConfigureEntityMapping(EntityTypeBuilder<MonitoringProjectApiQueryParameter> builder, IRelationalTypeMappingSource mappingSource)
    {
        builder.AutoConfigure(mappingSource);
        builder.ToTable(x => x.HasComment("ApiQuery参数"));
        builder.HasIndex(x => x.MonitoringProjectApiId);
    }
}