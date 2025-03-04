using Autofac;

namespace EchoTrace.IntegrationTests;

public static class TestEnvironmentCache
{
    public static ILifetimeScope? LifetimeScope { get; set; }

    /// <summary>
    /// 基础设施是否已经启动过
    /// </summary>
    public static bool IsInfrastructureStarted { get; set; } = false;
}