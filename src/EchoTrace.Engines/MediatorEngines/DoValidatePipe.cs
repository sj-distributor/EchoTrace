using Autofac;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Realization.Bases;
using FluentValidation;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;

namespace EchoTrace.Engines.MediatorEngines;

public class DoValidatePipe : IPipeSpecification<IReceiveContext<IMessage>>
{
    private readonly ILifetimeScope lifetimeScope;

    public DoValidatePipe(ILifetimeScope lifetimeScope)
    {
        this.lifetimeScope = lifetimeScope;
    }

    public bool ShouldExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        return true;
    }

    public Task BeforeExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        return Task.WhenAll();
    }

    public async Task Execute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        if (ShouldExecute(context, cancellationToken))
            if (context.Message != null)
            {
                var msgType = context.Message.GetType();
                var iValidatorType = typeof(IValidator<>).MakeGenericType(msgType);
                if (lifetimeScope.Resolve(iValidatorType) is IValidator validator &&
                    validator.CanValidateInstancesOfType(msgType))
                {
                    var result = await validator.ValidateAsync(new ContractValidationContext(context.Message),
                        cancellationToken);
                    if (!result.IsValid)
                    {
                        var validationMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
                        throw new BusinessException(validationMessages, BusinessExceptionTypeEnum.ParameterError);
                    }
                }
            }
    }

    public Task AfterExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
    {
        return Task.WhenAll();
    }

    public Task OnException(Exception ex, IReceiveContext<IMessage> context)
    {
        throw ex;
    }
}