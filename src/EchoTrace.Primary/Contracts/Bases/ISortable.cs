using System.ComponentModel;

namespace EchoTrace.Primary.Contracts.Bases;

public interface ISortable
{
    public SortParameter? Sort { get; set; }
}

public record SortParameter
{
    /// <summary>
    ///     字段名
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    ///     方向
    /// </summary>
    public SortDirectionEnum? Direction { get; set; }
}

public enum SortDirectionEnum
{
    /// <summary>
    ///     正序
    /// </summary>
    [Description("正序")] Ascending,

    /// <summary>
    ///     逆序
    /// </summary>
    [Description("逆序")] Descending
}