using EchoTrace.FilterAndMiddlewares;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers.Bases;

[ApiController]
[Authorize]
[ServiceFilter<AutoResolveFilter>]
[TypeFilter(typeof(HandleTimezoneResultFilter))]
public class BaseController : ControllerBase, IHasMediator
{
    public IMediator Mediator { get; set; }
}