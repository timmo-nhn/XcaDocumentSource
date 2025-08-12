using Hl7.Fhir.Model.CdsHooks;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging;
using Moq;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;

namespace XcaXds.UnitTests;

public class UnitTests_Fhir
{
    private readonly ILogger<XdsOnFhirService> _logger;
    private readonly ApplicationConfig _applicationConfig;
    private readonly FileBasedRegistry _fileRegistry;

    public UnitTests_Fhir()
    {
        var mockLogger = new Mock<ILogger<XdsOnFhirService>>();
        _logger = mockLogger.Object;

        _applicationConfig = new ApplicationConfig();

        _fileRegistry = new FileBasedRegistry(); 
    }


    [Fact]
    public async Task MHD_Iti67ToIti18AdhocQueryConversion()
    {
        var documentReferenceRequest = new MhdDocumentRequest()
        {
            Patient = "13116900216",
            Creation = "eq2019-01-14T16:55",
            Status = "current"
        };

        var xdsOnFhirService = new XdsOnFhirService(_fileRegistry,_applicationConfig,_logger);

        var adhocquery = xdsOnFhirService.ConvertIti67ToIti18AdhocQuery(documentReferenceRequest);

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var adhocquerystring = sxmls.SerializeSoapMessageToXmlString(adhocquery);
    }

    [Fact]
    public async Task MHD_TransformRegistryObjectsToFhirBundle()
    {
        var mockRegistry = new FileBasedRegistry();
        var xdsOnFhirService = new XdsOnFhirService(_fileRegistry, _applicationConfig, _logger);

        var registryObjects = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjects(mockRegistry.ReadRegistry());

        var rng = new Random();

        var randomAssociation = registryObjects.OfType<AssociationType>().PickRandom(8).ToList();

        var registryPackages = randomAssociation.Select(ra => registryObjects.GetById(ra?.SourceObject)).OfType<RegistryPackageType>().ToList();
        var extrinsicObjects = randomAssociation.Select(ra => registryObjects.GetById(ra?.TargetObject)).OfType<ExtrinsicObjectType>().ToList();

        var bundle = xdsOnFhirService.TransformRegistryObjectsToFhirBundle([..randomAssociation, ..registryPackages, ..extrinsicObjects]);
        var fhirJsonSerializer = new FhirJsonSerializer(new SerializerSettings() { Pretty = true });
        if (bundle != null)
        {
            var jsonOutput = fhirJsonSerializer.SerializeToString(bundle);
        }
    }
}