using Autofac;
using EchoTrace.Engines.Bases;
using EchoTrace.Infrastructure.Bases;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EchoTrace.DbMigration;

public class DbMigrationFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var builder = new ContainerBuilder();
        var container = builder.SceneBuildWithEngines(SceneOptions.WebApi);
        var sqlDbContext = container.Resolve<ApplicationDbContext>();
        return sqlDbContext;
    }
}