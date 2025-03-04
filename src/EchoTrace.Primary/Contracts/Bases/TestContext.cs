using Autofac;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;

namespace EchoTrace.Primary.Contracts.Bases;
public interface ITestContext
{
    ILifetimeScope LifetimeScope { get; set; }
    DbContext DbContext { get; set; }
    IMediator Mediator { get; set; }

    /// <summary>
    /// 是否不需要数据库
    /// </summary>
    public bool NoDatabase { get; set; }
}

public class TestContext<TMessage> : ITestContext
{
    public TestContext()
    {
        TestCases = new List<TestCase<TMessage>>();
    }

    public List<TestCase<TMessage>> TestCases { get; }

    public TestCase<TMessage> CreateTestCase()
    {
        var newCase = new TestCase<TMessage>();
        TestCases.Add(newCase);
        return newCase;
    }

    public ILifetimeScope LifetimeScope { get; set; }
    public DbContext DbContext { get; set; }
    public IMediator Mediator { get; set; }
    public bool NoDatabase { get; set; }
}

public class TestContext<TMessage, TResponse> : ITestContext
{
    public TestContext()
    {
        TestCases = new List<TestCase<TMessage, TResponse>>();
    }

    public ILifetimeScope LifetimeScope { get; set; }
    public DbContext DbContext { get; set; }
    public IMediator Mediator { get; set; }

    public bool NoDatabase { get; set; }
    public List<TestCase<TMessage, TResponse>> TestCases { get; }

    public TestCase<TMessage, TResponse> CreateTestCase()
    {
        var newCase = new TestCase<TMessage, TResponse>();
        TestCases.Add(newCase);
        return newCase;
    }
}

public interface ITestCase
{
    /// <summary>
    /// 执行之前是否需要清理数据库
    /// </summary>
    bool DatabaseCleanupRequired { get; set; }

    ApplicationUser? CurrentUser { get; set; }

    Action<ContainerBuilder>? Build { get; set; }

    Func<Task>? Arrange { get; set; }
}

public class TestCase<TMessage> : ITestCase
{
    public Action<ContainerBuilder>? Build { get; set; }
    public TMessage Message { get; set; }
    public Func<Task>? Arrange { get; set; }
    public Func<HandlerResult, Task>? Assert { get; set; }
    public bool DatabaseCleanupRequired { get; set; }

    public ApplicationUser? CurrentUser { get; set; }
}

public class HandlerResult
{
    public Exception? Exception { get; set; }
}

public class HandlerResult<T> : HandlerResult
{
    public T Response { get; set; }
}

public class TestCase<TMessage, TResponse> : ITestCase
{
    public Action<ContainerBuilder>? Build { get; set; }
    public TMessage Message { get; set; }
    public Func<Task>? Arrange { get; set; }
    public Func<HandlerResult<TResponse>, Task>? Assert { get; set; }
    public bool DatabaseCleanupRequired { get; set; }
    public ApplicationUser? CurrentUser { get; set; }
}