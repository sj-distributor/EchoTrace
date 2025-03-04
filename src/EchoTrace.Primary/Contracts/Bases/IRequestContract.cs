using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.Bases;

public interface IRequestContract<TRequest, TResponse> : IContract<TRequest>, ITestable<TRequest, TResponse>,
    IRequestHandler<TRequest, TResponse> where TRequest : class, IRequest where TResponse : class, IResponse
{
}