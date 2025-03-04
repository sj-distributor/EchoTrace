using Mediator.Net;

namespace EchoTrace.Controllers.Bases;

public interface IHasMediator
{
    IMediator Mediator { get; set; }
}