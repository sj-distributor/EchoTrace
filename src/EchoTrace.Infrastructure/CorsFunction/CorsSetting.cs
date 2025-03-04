using EchoTrace.Infrastructure.Bases;

namespace EchoTrace.Infrastructure.CorsFunction;

public class CorsSetting : IJsonFileSetting
{
    public string[] Origins { get; set; }
    public string JsonFilePath => "./CorsFunction/cors-setting.json";
}