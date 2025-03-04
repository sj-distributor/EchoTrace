namespace EchoTrace.Realization.Bases;

public class MapFromProfile<TSource, TTarget>
{
    public TSource Source { get; set; }

    public TTarget Target { get; set; }

    public bool IsIgnore { get; private set; }

    public T Value<T>(T value)
    {
        return value;
    }

    public void Ignore()
    {
        IsIgnore = true;
    }
}