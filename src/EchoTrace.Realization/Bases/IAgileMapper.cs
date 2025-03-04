namespace EchoTrace.Realization.Bases;

public interface IAgileMapper
{
    void Map<TSource, TTarget>(TSource source, TTarget? target,
        Action<AgileMapperConfiguration<TSource, TTarget>>? configure = null)
        where TSource : class where TTarget : class, new();

    TTarget Map<TSource, TTarget>(TSource source,
        Action<AgileMapperConfiguration<TSource, TTarget>>? configure = null)
        where TSource : class where TTarget : class, new();
}