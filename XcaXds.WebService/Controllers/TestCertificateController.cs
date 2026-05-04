using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Attributes;

namespace XcaXds.WebService.Controllers;

[ApiController]
[RequiresApiKey]
[Route("/secure")]
public class SecureController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("API Key required here");
}
