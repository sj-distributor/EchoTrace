using Autofac;

namespace EchoTrace.UnitTests;

public static class TestEnvironmentCache
{
    public static ILifetimeScope? LifetimeScope { get; set; }
}