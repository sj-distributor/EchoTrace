namespace EchoTrace.Primary.Bases;

public interface ICurrent
{
    Task<Guid> GetCurrentUserIdAsync();
}