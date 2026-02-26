using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging;
using Moq;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.DataManipulators;
using XcaXds.Source.Source;
using XcaXds.WebService.Services;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;

namespace XcaXds.Tests;

public class UnitTests_Fhir
{
    [Fact]
    public async Task MHD_Iti67ToIti18AdhocQueryConversion()
    {
        var documentReferenceRequest = new MhdDocumentRequest()
        {
            Patient = "13116900216",
            Creation = "eq2019-01-14T16:55",
            Status = "current"
        };

        var adhocquery = XdsOnFhirTransformer.ConvertIti67ToIti18AdhocQuery(documentReferenceRequest);

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var adhocquerystring = sxmls.SerializeSoapMessageToXmlString(adhocquery);
    }

    [Fact]
    public async Task MHD_TransformRegistryObjectsToFhirBundle()
    {
        var mockRegistry = new InMemoryRegistry();
        mockRegistry.WriteRegistry(TestHelpers.GenerateComprehensiveRegistryMetadata("13116900216", noDeprecatedDocuments: true).AsRegistryObjectList());

        var registryObjects = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects(mockRegistry.ReadRegistry().ToList());

        var rng = new Random();

        var randomAssociation = registryObjects.OfType<AssociationType>().PickRandom(8).ToList();

        var registryPackages = randomAssociation.Select(ra => registryObjects.GetById(ra?.SourceObject)).OfType<RegistryPackageType>().ToList();
        var extrinsicObjects = randomAssociation.Select(ra => registryObjects.GetById(ra?.TargetObject)).OfType<ExtrinsicObjectType>().ToList();

        var bundle = XdsOnFhirTransformer.TransformRegistryObjectsToFhirBundle([.. randomAssociation, .. registryPackages, .. extrinsicObjects], mockRegistry.ReadRegistry());
        var fhirJsonSerializer = new FhirJsonSerializer();
        if (bundle != null)
        {
            var jsonOutput = fhirJsonSerializer.SerializeToString(bundle);
        }
    }
}