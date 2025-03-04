using System.Linq.Expressions;
using System.Reflection;

namespace EchoTrace.Realization.Bases;

public class DestProperty<TSource, TDest, TProperty> where TSource : class where TDest : class, new()
{
    public PropertyInfo DestPropertyInfo { get; private set; }
    public PropertyInfo SourcePropertyInfo { get; private set; }
    public bool IsIgnored { get; private set; }
    public bool HasFixedValue { get; private set; }
    public TProperty? FixedValue { get; private set; }

    public DestProperty<TSource, TDest, TProperty> Dest(Expression<Func<TDest, TProperty>> expression)
    {
        if (expression.Body is MemberExpression { Member: PropertyInfo destPropertyInfo })
            DestPropertyInfo = destPropertyInfo;

        return this;
    }

    public void Source(Expression<Func<TSource, TProperty>> expression)
    {
        if (expression.Body is MemberExpression { Member: PropertyInfo sourcePropertyInfo })
            SourcePropertyInfo = sourcePropertyInfo;
    }

    public void Value(TProperty? value)
    {
        HasFixedValue = true;
        FixedValue = value;
    }

    public void Ignore(bool isIgnore = true)
    {
        IsIgnored = isIgnore;
    }
}