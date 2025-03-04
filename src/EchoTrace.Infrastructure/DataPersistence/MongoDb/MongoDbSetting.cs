using EchoTrace.Infrastructure.Bases;

namespace EchoTrace.Infrastructure.DataPersistence.MongoDb;

public class MongoDbSetting : IJsonFileSetting
{
    public MongoDbServerSetting[] Servers { get; set; }
    public MongoDatabaseNamesSetting DatabaseNames { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string JsonFilePath => "./DataPersistence/MongoDb/mongodb-setting.json";
}

public class MongoDatabaseNamesSetting
{
    public string Authorized { get; set; }
    public string WebApi { get; set; }
    public string IntegrationTest { get; set; }
}

public class MongoDbServerSetting
{
    public string Host { get; set; }
    public int Port { get; set; }
}