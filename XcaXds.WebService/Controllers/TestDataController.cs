using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;


[ApiController]
[UsePolicyEnforcementPoint]
[Route("api/rest")]
[Consumes("application/json")]
[Produces("application/json")]
public class TestDataController : ControllerBase
{

    [HttpPost("generate-testdata")]
    public async Task<IActionResult> GenerateTestData(int numberOfEntries, [FromBody] Test_DocumentReferenceValue testDocumentReference)
    {
        

        return Ok("hibb");
    }

}
