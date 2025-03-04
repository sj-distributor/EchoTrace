namespace EchoTrace.Primary.Bases;

public interface ICurrentSingle<TEntity> : ICurrent
{
    Task<TEntity> QueryAsync(CancellationToken cancellationToken = default);
}