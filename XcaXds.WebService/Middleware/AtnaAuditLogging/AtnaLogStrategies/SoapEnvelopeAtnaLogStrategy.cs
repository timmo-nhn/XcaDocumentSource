using Microsoft.AspNetCore.Http;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilderStrategies;

public class SoapEnvelopeAtnaLogStrategy : IAtnaLogStrategy
{
    public bool CanHandle(string contentType, string method)
    {
        if (contentType == Constants.MimeTypes.SoapXml)
        {
            return true;
        }

        return false;
    }

    public Task<AtnaLogBuilderResult> BuildAsync(HttpContext context)
    {
        throw new NotImplementedException();
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var requestSoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(context.Request.Body);
        var responseSoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(context.Response.Body);
        
    }
}