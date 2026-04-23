using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XcaXds.WebService.Controllers;

[Authorize(AuthenticationSchemes = CertificateAuthenticationDefaults.AuthenticationScheme)]
[ApiController]
[Route("/secure")]
public class SecureController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Client cert required here");
}
