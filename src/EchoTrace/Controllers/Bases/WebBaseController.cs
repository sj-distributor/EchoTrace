using EchoTrace.Engines.SwaggerEngines;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Controllers.Bases;

[Route("api/web/[controller]")]
[SwaggerApiGroup(SwaggerApiGroupNames.Web)]
public class WebBaseController : BaseController
{
}