namespace EchoTrace.Primary.Contracts.Bases;

public interface IPageable
{
    public int Offset { get; set; }

    public int PageSize { get; set; }
}