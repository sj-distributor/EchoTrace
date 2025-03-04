using EchoTrace.Infrastructure.Bases;

namespace EchoTrace.Infrastructure.JwtFunction;

public class JwtSetting : IJsonFileSetting
{
    /// <summary>
    ///     签发者
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    ///     签名秘钥
    /// </summary>
    public string SignKey { get; set; }

    /// <summary>
    ///     使用者
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    ///     长过期时间
    /// </summary>
    public int LongExpiresInMinutes { get; set; }

    /// <summary>
    ///     短过期时间
    /// </summary>
    public int ShortExpiresInMinutes { get; set; }

    public string JsonFilePath => "./JwtFunction/jwt-setting.json";
}