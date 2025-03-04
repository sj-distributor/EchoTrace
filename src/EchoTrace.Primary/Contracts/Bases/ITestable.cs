using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.Bases;

public interface ITestable
{
}

public interface ITestable<TMessage> : ITestable where TMessage : IMessage
{
    void Test(TestContext<TMessage> context);
}

public interface ITestable<TMessage, TResponse> : ITestable where TMessage : IMessage where TResponse : IResponse
{
    void Test(TestContext<TMessage, TResponse> context);
}