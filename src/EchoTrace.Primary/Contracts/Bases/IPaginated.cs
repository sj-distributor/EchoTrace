namespace EchoTrace.Primary.Contracts.Bases;

public interface IPaginated
{
    /// <summary>
    ///     总数
    /// </summary>
    public int Total { get; set; }
}