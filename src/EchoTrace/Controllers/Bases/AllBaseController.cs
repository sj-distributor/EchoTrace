using EchoTrace.Engines.SwaggerEngines;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers.Bases;

[Route("api/app/[controller]")]
[Route("api/web/[controller]")]
[SwaggerApiGroup(true)]
public class AllBaseController : BaseController
{
}