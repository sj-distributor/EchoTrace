namespace EchoTrace.Infrastructure.DataPersistence.DataEntityBases;

public interface IPaginated<TItem>
{
    /// <summary>
    ///     总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    ///     列表
    /// </summary>
    public List<TItem> List { get; set; }
}