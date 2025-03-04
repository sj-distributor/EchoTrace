using System.Reflection;

namespace EchoTrace.Realization.Bases;

public class MapPropertyEvent
{
    public MapPropertyEvent()
    {
        Functions = new List<Delegate>();
        Actions = new List<Delegate>();
    }

    public PropertyInfo PropertyInfo { get; set; }

    public List<Delegate> Functions { get; set; }

    public List<Delegate> Actions { get; set; }
}