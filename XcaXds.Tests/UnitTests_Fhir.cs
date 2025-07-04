using Microsoft.Extensions.Logging;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Services;

namespace XcaXds.Tests;

public class UnitTests_Fhir
{
    private readonly ILogger<XdsOnFhirService> _logger;

    [Fact]
    public async Task TestIti67ToIti18AdhocQueryConversion()
    {
        var documentReferenceRequest = new MhdDocumentRequest()
        {
            Patient = "13116900216",
            Creation = "eq2019-01-14T16:55",
            Status = "current"
        };

        var _xdsOnFhirService = new XdsOnFhirService(_logger);

        var adhocquery = _xdsOnFhirService.ConvertIti67ToIti18AdhocQuery(documentReferenceRequest);

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var adhocquerystring = sxmls.SerializeSoapMessageToXmlString(adhocquery);
    }
}