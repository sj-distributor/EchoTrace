namespace EchoTrace.Realization.Bases;

public class PropertyEventEqualityComparer : IEqualityComparer<MapPropertyEvent>
{
    public bool Equals(MapPropertyEvent? x, MapPropertyEvent? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.PropertyInfo == y.PropertyInfo ||
               (x.PropertyInfo.PropertyType == y.PropertyInfo.PropertyType &&
                x.PropertyInfo.Name == y.PropertyInfo.Name);
    }

    public int GetHashCode(MapPropertyEvent obj)
    {
        return HashCode.Combine(obj.PropertyInfo.PropertyType, obj.PropertyInfo.Name);
    }
}