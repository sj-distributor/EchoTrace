using AutoMapper;

namespace EchoTrace.Primary.Bases;

public interface IMapFrom<in TSource> where TSource : class
{
    virtual void ConfigureMapper(IMapperConfigurationExpression cfg, TSource? source)
    {
    }
}