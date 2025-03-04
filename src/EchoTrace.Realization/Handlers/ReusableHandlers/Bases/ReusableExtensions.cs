using Autofac;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;

namespace EchoTrace.Realization.Handlers.ReusableHandlers.Bases;

public static class ReusableExtensions
{
    public static async Task HandleAsync<TParameter>(this TParameter parameter,
        CancellationToken cancellationToken = default)
        where TParameter : IReusableHandlerParameter
    {
        if (CurrentApplication.TryContextResolve<IReusableHandler<TParameter>>(out var handler) && handler is not null)
        {
            await handler.Handle(parameter, cancellationToken);
            return;
        }

        throw new InvalidOperationException(
            $"Current reusable parameter {typeof(TParameter).Name} does not have a corresponding handler");
    }

    public static async Task<TReturn> HandleAsync<TParameter, TReturn>(this TParameter parameter,
        CancellationToken cancellationToken = default)
        where TParameter : IReusableHandlerParameter where TReturn : IReusableHandlerReturn
    {
        if (CurrentApplication.TryContextResolve<IReusableHandler<TParameter, TReturn>>(out var handler) &&
            handler is not null)
        {
            return await handler.Handle(parameter, cancellationToken);
        }

        throw new InvalidOperationException(
            $"Current reusable parameter '{typeof(TParameter).Name} + {typeof(TReturn).Name}' does not have a corresponding handler");
    }

    public static async Task ReuseHandleAsync<TParameter>(this IReceiveContext<IMessage> context, TParameter parameter,
        CancellationToken cancellationToken = default) where TParameter : IReusableHandlerParameter
    {
        if (context.TryGetService<IMediator>(out var mediator))
        {
            var response =
                await mediator.RequestAsync<GetCurrentLifetimeScopeRequest, GetCurrentLifetimeScopeResponse>(
                    new GetCurrentLifetimeScopeRequest(), cancellationToken);
            var scope = response.LifetimeScope;
            var reusableHandler = scope.Resolve<IReusableHandler<TParameter>>();
            await reusableHandler.Handle(parameter, cancellationToken);
        }
    }

    public static async Task<TReturn> ReuseHandleAsync<TParameter, TReturn>(this IReceiveContext<IMessage> context,
        TParameter parameter,
        CancellationToken cancellationToken = default) where TParameter : IReusableHandlerParameter
        where TReturn : IReusableHandlerReturn
    {
        if (context.TryGetService<IMediator>(out var mediator))
        {
            var response =
                await mediator.RequestAsync<GetCurrentLifetimeScopeRequest, GetCurrentLifetimeScopeResponse>(
                    new GetCurrentLifetimeScopeRequest(), cancellationToken);
            var scope = response.LifetimeScope;
            var reusableHandler = scope.Resolve<IReusableHandler<TParameter, TReturn>>();
            var result = await reusableHandler.Handle(parameter, cancellationToken);
            return result;
        }

        return default!;
    }
}