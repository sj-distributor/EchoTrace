using EchoTrace.Controllers.Bases;
using Mediator.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EchoTrace.FilterAndMiddlewares;

public class AutoResolveFilter(IMediator mediator) : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller;
        if (controller is IHasMediator mediatorController)
        {
            mediatorController.Mediator = mediator;
        }

        return next();
    }
}