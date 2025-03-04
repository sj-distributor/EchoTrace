using EchoTrace.Engines.SwaggerEngines;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers.Bases;

[Route("api/app/[controller]")]
[SwaggerApiGroup(SwaggerApiGroupNames.App)]
public class AppBaseController : BaseController
{
}