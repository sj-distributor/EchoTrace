using EchoTrace.Infrastructure.Bases;

namespace EchoTrace.Infrastructure.SeqLog;

public class SeqSetting : IJsonFileSetting
{
    public string ServerUrl { get; set; }
    public string ApiKey { get; set; }
    public string JsonFilePath => "./SeqLog/seq-setting.json";
}