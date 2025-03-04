using System.Reflection;

namespace EchoTrace.Realization.Bases;

public interface IPropertyMapper<TSource, TDest, TProperty> where TSource : class where TDest : class, new()
{
    public PropertyInfo DestPropertyInfo { get; set; }

    public PropertyInfo? SourcePropertyInfo { get; set; }

    public bool IsIgnored { get; set; }

    TProperty FixedValue { get; set; }
}