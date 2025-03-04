using EchoTrace.Infrastructure.DataPersistence.DataEntityBases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.Bases;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;

public class ApplicationUser : IEfEntity<ApplicationUser>, IHasKey<Guid>, IHasCreatedOn
{
    private ApplicationUser()
    {
        this.InitPropertyValues();
    }

    public ApplicationUser(string userName, string passwordHash)
    {
        this.InitPropertyValues();
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        this.UserName = userName;
        this.PasswordHash = passwordHash;
    }

    public static void ConfigureEntityMapping(EntityTypeBuilder<ApplicationUser> builder,
        IRelationalTypeMappingSource mappingSource)
    {
        builder.AutoConfigure(mappingSource);
    }

    public Guid Id { get; set; }
    
    public string UserName { get; set; }
    
    public string PasswordHash { get; set; }
    
    public DateTime CreatedOn { get; set; }
}