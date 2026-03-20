using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilderStrategies;

public class SoapEnvelopeAtnaLogBuilderStrategy : IAtnaLogBuilderStrategy
{
    public bool CanHandle(string? urlPath, string httpMethod)
    {
        throw new NotImplementedException();
    }

    public Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry)
    {


        throw new NotImplementedException();
    }
}
