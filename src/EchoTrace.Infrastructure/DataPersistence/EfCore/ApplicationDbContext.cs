using System.Diagnostics;
using EchoTrace.Infrastructure.Bases;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EchoTrace.Infrastructure.DataPersistence.EfCore;

public class ApplicationDbContext : DbContext
{
    private readonly DbSetting _dbSetting;
    private readonly SettingOptions _settingOptions;

    public ApplicationDbContext(DbSetting dbSetting, SettingOptions settingOptions)
    {
        _dbSetting = dbSetting;
        _settingOptions = settingOptions;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectString = _settingOptions.Scene == SceneOptions.Test
            ? _dbSetting.ConnectionStrings.IntegrationTest
            : _dbSetting.ConnectionStrings.WebApi;

        optionsBuilder.UseMySql(connectString, new MySqlServerVersion(new Version(8, 0)),
            options => { options.CommandTimeout(6000); });
        if (Debugger.IsAttached)
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var mappingSource = this.GetService<IRelationalTypeMappingSource>();
        modelBuilder.LoadFromEntityConfigure(mappingSource);
    }
}