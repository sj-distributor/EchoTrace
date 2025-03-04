namespace EchoTrace.Engines.SwaggerEngines;

[AttributeUsage(AttributeTargets.Field)]
public class SwaggerGroupInfoAttribute : Attribute
{
    /// <summary>
    ///     标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     匹配规则
    /// </summary>
    public string MatchRule { get; set; }
}