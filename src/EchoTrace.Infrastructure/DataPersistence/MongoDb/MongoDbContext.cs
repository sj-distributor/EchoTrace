using EchoTrace.Infrastructure.Bases;
using EchoTrace.Infrastructure.DataPersistence.MongoDb;
using MongoDB.Driver;
using Serilog;

namespace EchoTrace.Infrastructure.MongoDb;

public class MongoDbContext
{
    public MongoDbContext(MongoDbSetting mongoDbSetting, SettingOptions settingOptions)
    {
        MongoCredential? credential = null;
        var databaseName = "";
        if (settingOptions.Scene == SceneOptions.Test)
        {
            databaseName = mongoDbSetting.DatabaseNames.IntegrationTest;
        }
        else if (settingOptions.Scene == SceneOptions.WebApi)
        {
            databaseName = mongoDbSetting.DatabaseNames.WebApi;
        }

        if (!string.IsNullOrWhiteSpace(mongoDbSetting.Username) && !string.IsNullOrWhiteSpace(mongoDbSetting.Password))
        {
            credential = MongoCredential.CreateCredential(mongoDbSetting.DatabaseNames.Authorized,
                mongoDbSetting.Username,
                mongoDbSetting.Password);
        }

        var settings = new MongoClientSettings
        {
            Servers = mongoDbSetting.Servers.Select(e => new MongoServerAddress(e.Host, e.Port)),
            Credential = credential,
            DirectConnection = settingOptions.Scene == SceneOptions.Test
        };

        try
        {
            var client = new MongoClient(settings);
            Client = client;
            Database = client.GetDatabase(databaseName);
        }
        catch (Exception e)
        {
            Log.Warning("Connect to mongodb failed: {Message}", e.Message);
        }
    }

    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }
}