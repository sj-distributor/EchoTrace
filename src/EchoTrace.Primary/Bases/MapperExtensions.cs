using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace EchoTrace.Primary.Bases;

public static class MapperExtensions
{
    public static IMappingExpression<TSource, TDest> CreateMapperConfiguration<TDest, TSource>(this TDest dest,
        IMapperConfigurationExpression cfg, TSource? source)
        where TSource : class
        where TDest : IMapFrom<TSource>
    {
        return cfg.CreateMap<TSource, TDest>();
    }

    public static IQueryable<TDest> SelectTo<TEntity, TDest>(this IQueryable<TEntity> queryable, TDest dest)
        where TEntity : class
        where TDest : IMapFrom<TEntity>
    {
        var mapper = TryGetMapper();
        return queryable.ProjectTo<TDest>(mapper.ConfigurationProvider);
    }

    public static TDest MapFromSource<TDest, TSource>(this TDest dest, TSource source)
        where TSource : class
        where TDest : IMapFrom<TSource>
    {
        var mapper = TryGetMapper();
        return mapper.Map(source, dest);
    }

    public static List<TDest> MapFromSource<TDest, TSource>(this List<TDest> destList,
        IEnumerable<TSource> sources)
        where TSource : class
        where TDest : IMapFrom<TSource>
    {
        var mapper = TryGetMapper();
        return mapper.Map(sources, destList);
    }

    public static TSource MapToSource<TDest, TSource>(this TDest dest, TSource source)
        where TSource : class
        where TDest : IMapFrom<TSource>
    {
        var mapper = TryGetMapper();
        return mapper.Map(dest, source);
    }

    public static IEnumerable<TSource> MapToSource<TDest, TSource>(this List<TDest> destList,
        IEnumerable<TSource> sources)
        where TSource : class
        where TDest : IMapFrom<TSource>
    {
        var mapper = TryGetMapper();
        return mapper.Map(destList, sources);
    }

    private static IMapper TryGetMapper()
    {
        if (CurrentApplication.TryContextResolve<IMapper>(out var mapper) && mapper != null)
        {
            return mapper;
        }

        throw new InvalidOperationException(
            "Mapper service is not available. Please ensure that the mapper service is registered and resolved correctly.");
    }
}