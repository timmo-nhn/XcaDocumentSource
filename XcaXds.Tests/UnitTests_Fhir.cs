using Hl7.Fhir.Model.CdsHooks;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging;
using Moq;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;

namespace XcaXds.Tests;

public class UnitTests_Fhir
{
    [Fact]
    public async Task TestIti67ToIti18AdhocQueryConversion()
    {
        var documentReferenceRequest = new MhdDocumentRequest()
        {
            Patient = "13116900216",
            Creation = "eq2019-01-14T16:55",
            Status = "current"
        };


        var adhocquery = XdsOnFhirService.ConvertIti67ToIti18AdhocQuery(documentReferenceRequest);

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var adhocquerystring = sxmls.SerializeSoapMessageToXmlString(adhocquery);
    }

    [Fact]
    public async Task TestTransformRegistryObjectsToFhirBundle()
    {
        var mockRegistry = new FileBasedRegistry();

        var registryObjects = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjects(mockRegistry.ReadRegistry());

        var bundle = XdsOnFhirService.TransformRegistryObjectsToFhirBundle(registryObjects);
    }
}