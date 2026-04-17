using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
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
        mockRegistry.WriteRegistry(TestHelpers.GeneratePotentiallyFaultyComprehensiveRegistryMetadata(10, "13116900216", noDeprecatedDocuments: true).AsRegistryObjectDtos().ToList());

        var registryObjects = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects((await mockRegistry.ReadRegistry().ToListAsync())!);

        var rng = new Random();

        var randomAssociation = registryObjects.OfType<AssociationType>().PickRandom(8).ToList();

        var registryPackages = randomAssociation.Select(ra => registryObjects.GetById(ra?.SourceObject ?? "")).OfType<RegistryPackageType>().ToList();
        var extrinsicObjects = randomAssociation.Select(ra => registryObjects.GetById(ra?.TargetObject ?? "")).OfType<ExtrinsicObjectType>().ToList();

        var bundle = XdsOnFhirTransformer.TransformRegistryObjectsToFhirBundle([.. randomAssociation, .. registryPackages, .. extrinsicObjects], mockRegistry.ReadRegistry().ToBlockingEnumerable());
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

        var fhirProvideBundle01 = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")) ?? "");
        var fhirProvideBundle02 = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle02.json")) ?? "");
        var fhirProvideBundle01WrongValues = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01_WrongValues.json")) ?? "");

        var fhirjsonDeserializer = new FhirJsonDeserializer();

        var bundle01 = fhirjsonDeserializer.Deserialize<Bundle>(fhirProvideBundle01);
        var bundle02 = fhirjsonDeserializer.Deserialize<Bundle>(fhirProvideBundle02);
        var bundleWrongValues = fhirjsonDeserializer.Deserialize<Bundle>(fhirProvideBundle01WrongValues);

        var fhirValidator = new FhirResourceValidatorService(new FakeLogger<FhirResourceValidatorService>(), new Mock<ApplicationConfig>().Object);

        var fhirJsonSerializer = new FhirJsonSerializer();

        var validationResult01 = fhirValidator.ValidateFhirResource(bundle01);
        var jsonResponse = fhirJsonSerializer.SerializeToString(validationResult01);
        Assert.DoesNotContain(OperationOutcome.IssueSeverity.Error, validationResult01.Issue.Select(iss => iss.Severity));


        var validationResult02 = fhirValidator.ValidateFhirResource(bundle02);
        Assert.DoesNotContain(OperationOutcome.IssueSeverity.Error, validationResult02.Issue.Select(iss => iss.Severity));

        var validationResult01WrongValues = fhirValidator.ValidateFhirResource(bundleWrongValues);
        Assert.Contains(OperationOutcome.IssueSeverity.Error, validationResult01WrongValues.Issue.Select(iss => iss.Severity));

        jsonResponse = fhirJsonSerializer.SerializeToString(validationResult01);
    }
}