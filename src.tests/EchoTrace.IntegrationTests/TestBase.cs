using System.Text;
using Autofac;
using EchoTrace.Engines.Bases;
using EchoTrace.Infrastructure.DataPersistence.EfCore;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using EchoTrace.Infrastructure.DataPersistence.MongoDb;
using EchoTrace.Infrastructure.MongoDb;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MySqlConnector;
using NSubstitute;

namespace EchoTrace.IntegrationTests;

[Collection("Sequential")]
public class TestBase : IClassFixture<SequentialCollectionFixture>, IAsyncDisposable, IDisposable
{
    public TestBase()
    {
        Build();
    }

    protected ILifetimeScope TestLifetimeScope { get; private set; }
    protected IMediator TestMediator { get; private set; }
    protected ApplicationDbContext TestDbContext { get; private set; }

    protected void Build(ITestCase? testCase = null)
    {
        if (TestEnvironmentCache.LifetimeScope == null)
        {
            var builder = new ContainerBuilder();
            TestEnvironmentCache.LifetimeScope = builder.TestBuildWithEngines(containerBuilder =>
                BuildContainerByTestCase(testCase, containerBuilder));
        }

        TestLifetimeScope = TestEnvironmentCache.LifetimeScope.BeginLifetimeScope(containerBuilder =>
            BuildContainerByTestCase(testCase, containerBuilder));
        TestMediator = TestLifetimeScope.Resolve<IMediator>();
        TestDbContext = TestLifetimeScope.Resolve<ApplicationDbContext>();
    }

    private void BuildContainerByTestCase(ITestCase? testCase, ContainerBuilder containerBuilder)
    {
        if (testCase is { CurrentUser: not null })
        {
            containerBuilder.Register(_ =>
                {
                    var currentUser = Substitute.For<ICurrentSingle<ApplicationUser>>();
                    currentUser.GetCurrentUserIdAsync().ReturnsForAnyArgs(testCase.CurrentUser.Id);
                    currentUser.QueryAsync().ReturnsForAnyArgs(testCase.CurrentUser);
                    return currentUser;
                })
                .As<ICurrentSingle<ApplicationUser>>()
                .InstancePerLifetimeScope();
        }

        if (testCase is { Build: not null })
        {
            testCase.Build(containerBuilder);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }

    private async Task<ApplicationDbContext?> CheckDbConnect()
    {
        if (TestLifetimeScope.TryResolve<DbSetting>(out var dbSetting))
        {
            if (!string.IsNullOrWhiteSpace(dbSetting.ConnectionStrings?.IntegrationTest))
            {
                var dbContext = TestLifetimeScope.Resolve<ApplicationDbContext>();
                // 为了区分mock的和非mock的
                if (dbContext.GetType().FullName!.Contains("EchoTrace"))
                {
                    var connectString = dbContext.Database.GetConnectionString();
                    if (!string.IsNullOrWhiteSpace(connectString))
                    {
                        if (await dbContext.Database.CanConnectAsync())
                        {
                            return dbContext;
                        }

                        var sqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectString);
                        var createDbSql = GetCreateDbSql(sqlConnectionStringBuilder);
                        sqlConnectionStringBuilder.Database = "mysql";
                        await using var connection = new MySqlConnection(sqlConnectionStringBuilder.ToString());
                        await connection.OpenAsync();
                        await using var command = new MySqlCommand(createDbSql, connection);
                        await command.ExecuteNonQueryAsync();
                        await dbContext.Database.OpenConnectionAsync();
                        if (await dbContext.Database.CanConnectAsync())
                        {
                            return dbContext;
                        }
                    }
                }
            }
        }

        return null;
    }

    private string GetCreateDbSql(MySqlConnectionStringBuilder sqlConnectionStringBuilder)
    {
        return $"CREATE DATABASE IF NOT EXISTS {sqlConnectionStringBuilder.Database};";
    }

    protected async Task StartupInfrastructure()
    {
        if (!TestEnvironmentCache.IsInfrastructureStarted)
        {
            await StartupSqlDb();
            TestEnvironmentCache.IsInfrastructureStarted = true;
        }
    }

    private async Task StartupSqlDb()
    {
        var db = await CheckDbConnect();
        if (db is not null)
        {
            var migrations = db.Database.GetMigrations();
            if (migrations.Any())
            {
                await db.Database.MigrateAsync();
            }
        }
    }

    protected async Task CleanupInfrastructure()
    {
        await CleanDbTables();
        await CleanMongoDbCollections();
    }

    private async Task CleanMongoDbCollections()
    {
        var mongoDatabase = CheckMongoDbConnection();
        if (mongoDatabase != null)
        {
            var collectionNames = await (await mongoDatabase.ListCollectionNamesAsync()).ToListAsync();
            foreach (var collectionName in collectionNames)
            {
                var collection = mongoDatabase.GetCollection<BsonDocument>(collectionName);
                var filter = Builders<BsonDocument>.Filter.Empty;
                await collection.DeleteManyAsync(filter);
            }
        }
    }

    private IMongoDatabase? CheckMongoDbConnection()
    {
        if (TestLifetimeScope.TryResolve<MongoDbSetting>(out var mongoSetting))
        {
            if (mongoSetting.Servers is { Length: > 0 })
            {
                var mongoDbContext = TestLifetimeScope.Resolve<MongoDbContext>();

                if (mongoDbContext is { Database: not null })
                {
                    if (mongoDbContext.GetType().FullName!.Contains("EchoTrace"))
                    {
                        return mongoDbContext.Database;
                    }
                }
            }
        }

        return null;
    }

    private async Task CleanDbTables()
    {
        var db = await CheckDbConnect();
        if (db is not null)
        {
            var tableTypes = db.Model.GetEntityTypes();
            var sqlBuilder = new StringBuilder();
            foreach (var tableType in tableTypes)
            {
                sqlBuilder.AppendLine($"DELETE FROM {tableType.GetTableName()};");
            }

            await db.Database.ExecuteSqlRawAsync(sqlBuilder.ToString());
        }
    }

    public void Dispose()
    {
        TestLifetimeScope.Dispose();
        TestMediator.Dispose();
        TestDbContext.Dispose();
    }
}