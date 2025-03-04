namespace EchoTrace.Infrastructure.DataPersistence.EfCore.Bases;

public class DynamicFilterParameter
{
    public string? FieldName { get; set; }
    public string? Operator { get; set; }
    public object? Value { get; set; }
}

public class DynamicFilterOperator
{
    /// <summary>
    /// 相等
    /// </summary>
    public const string Equal = "eq";

    /// <summary>
    /// 不相等
    /// </summary>
    public const string NotEqual = "neq";

    /// <summary>
    /// 大于
    /// </summary>
    public const string GreaterThan = "gt";

    /// <summary>
    /// 大于等于
    /// </summary>
    public const string GreaterThanOrEqual = "gte";

    /// <summary>
    /// 小于
    /// </summary>
    public const string LessThan = "lt";

    /// <summary>
    /// 小于等于
    /// </summary>
    public const string LessThanOrEqual = "lte";

    /// <summary>
    /// 包含
    /// </summary>
    public const string Contains = "ct";
}