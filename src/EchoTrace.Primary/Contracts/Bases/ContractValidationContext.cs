using FluentValidation;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.Bases;

public class ContractValidationContext : ValidationContext<IMessage>
{
    public ContractValidationContext(IMessage message) : base(message)
    {
    }
}