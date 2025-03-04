using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.Bases;

public interface IContract<T> where T : IMessage
{
    void Validate(ContractValidator<T> validator);
}