using FluentValidation;
using Mediator.Net.Contracts;

namespace EchoTrace.Primary.Contracts.Bases;

public class ContractValidator<TMessage> : AbstractValidator<TMessage> where TMessage : IMessage
{
}