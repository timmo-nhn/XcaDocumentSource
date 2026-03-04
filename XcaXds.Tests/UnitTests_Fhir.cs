using Castle.Core.Logging;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using Task = System.Threading.Tasks.Task;

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

        var statusSlot = adhocquery.AdhocQuery.GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.Status);

        Assert.Equal(Constants.Xds.StatusValues.Approved, statusSlot?.GetFirstValue());
    }

    [Fact]
    public async Task MHD_TransformRegistryObjectsToFhirBundle()
    {
        var mockRegistry = new InMemoryRegistry();
        mockRegistry.WriteRegistry(TestHelpers.GeneratePotentiallyFaultyComprehensiveRegistryMetadata(10, "13116900216", noDeprecatedDocuments: true).AsRegistryObjectList());

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

    [Fact]
    public async Task FhirPath_Testing_ValidateBundle()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")) ?? "");
        var fhirProvideBundleWrongValues = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01_WrongValues.json")) ?? "");

        var fhirjsonDeserializer = new FhirJsonDeserializer();

        var bundle = fhirjsonDeserializer.Deserialize<Bundle>(fhirProvideBundle);
        var bundleWrongValues = fhirjsonDeserializer.Deserialize<Bundle>(fhirProvideBundleWrongValues);

        var fhirValidator = new FhirResourceValidatorService(new FakeLogger<FhirResourceValidatorService>(), new Mock<ApplicationConfig>().Object);

        var validationResult = fhirValidator.ValidateFhirResource(bundle);
        var validationResultWrongValues = fhirValidator.ValidateFhirResource(bundleWrongValues);
    }
}