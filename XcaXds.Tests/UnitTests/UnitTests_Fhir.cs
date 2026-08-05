using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests.UnitTests;

public class UnitTests_Fhir(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : IntegrationTests_DefaultFixture(factory, output), IClassFixture<WebApplicationFactory<WebService.Program>>
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

        var adhocquery = _xdsOnFhirTransformerService.ConvertIti67ToIti18AdhocQuery(documentReferenceRequest);

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var adhocquerystring = sxmls.SerializeSoapMessageToXmlString(adhocquery);

        var statusSlot = adhocquery.AdhocQuery.GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.Status);

        Assert.Equal(Constants.Xds.StatusValues.Approved, statusSlot?.GetFirstValue());
    }

    [Fact]
    public async Task MHD_ConvertBundleToIti41()
    {
        var documentReferenceRequest = new MhdDocumentRequest()
        {
            Patient = "13116900216",
            Creation = "eq2019-01-14T16:55",
            Status = "current"
        };

        var adhocquery = _xdsOnFhirTransformerService.ConvertIti67ToIti18AdhocQuery(documentReferenceRequest);

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

        var registryObjects = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjectsStateless((await mockRegistry.ReadRegistry().ToListAsync(TestContext.Current.CancellationToken))!);

        var rng = new Random();

        var randomAssociation = registryObjects.OfType<AssociationType>().PickRandom(8).ToList();

        var registryPackages = randomAssociation.Select(ra => registryObjects.GetById(ra?.SourceObject ?? "")).OfType<RegistryPackageType>().ToList();
        var extrinsicObjects = randomAssociation.Select(ra => registryObjects.GetById(ra?.TargetObject ?? "")).OfType<ExtrinsicObjectType>().ToList();

        var bundle = _xdsOnFhirTransformerService.TransformRegistryObjectsToFhirBundle([.. randomAssociation, .. registryPackages, .. extrinsicObjects], mockRegistry.ReadRegistry().ToBlockingEnumerable(TestContext.Current.CancellationToken));
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

        var fhirJsonSerializer = new FhirJsonSerializer();

        var validationResult01 = _fhirResourceValidatorService.ValidateFhirResource(bundle01);
        var jsonResponse = fhirJsonSerializer.SerializeToString(validationResult01);
        Assert.DoesNotContain(OperationOutcome.IssueSeverity.Error, validationResult01.Issue.Select(iss => iss.Severity));


        var validationResult02 = _fhirResourceValidatorService.ValidateFhirResource(bundle02);
        Assert.DoesNotContain(OperationOutcome.IssueSeverity.Error, validationResult02.Issue.Select(iss => iss.Severity));

        var validationResult01WrongValues = _fhirResourceValidatorService.ValidateFhirResource(bundleWrongValues);
        Assert.Contains(OperationOutcome.IssueSeverity.Error, validationResult01WrongValues.Issue.Select(iss => iss.Severity));

        jsonResponse = fhirJsonSerializer.SerializeToString(validationResult01);
    }

    [Fact]
    public void GetDocumentReferenceAuthors_WithMultipleRoles_DoesNotAccumulateRoleSlots()
    {
        var transformer = _scope.ServiceProvider.GetRequiredService<FhirToXdsTransformerService>();
        var additionalRoleDisplay = "AdditionalRoleForTest";
        var documentReference = LoadDocumentReferenceWithSecondAuthorRole(additionalRoleDisplay);

        var classifications = InvokeGetDocumentReferenceAuthors(transformer, documentReference, out _);

        Assert.Equal(2, classifications.Length);

        var roleValues = classifications
            .Select(classification => classification.Slot
                .Where(slot => slot.Name == Constants.Xds.SlotNames.AuthorRole)
                .SelectMany(slot => slot.GetValues(codeMultipleValues: false) ?? [])
                .ToArray())
            .Select(values => values ?? [])
            .ToList();

        var additionalRoleClassificationValues = roleValues.Single(values => values.Contains(additionalRoleDisplay));
        Assert.Single(additionalRoleClassificationValues);
    }

    private static ClassificationType[] InvokeGetDocumentReferenceAuthors(
        FhirToXdsTransformerService transformer,
        DocumentReference documentReference,
        out OperationOutcome operationOutcome)
    {
        var method = typeof(FhirToXdsTransformerService)
            .GetMethod("GetDocumentReferenceAuthors", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        var arguments = new object?[] { documentReference, null };
        var result = method!.Invoke(transformer, arguments) as ClassificationType[];

        operationOutcome = arguments[1] as OperationOutcome ?? new OperationOutcome();
        return result ?? [];
    }

    private static DocumentReference LoadDocumentReferenceWithSecondAuthorRole(string additionalRoleDisplay)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var fhirFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var bundleJson = File.ReadAllText(fhirFiles.First(file => file.Contains("ProvideBundle02_dept_with_reference_in_authors.json")));

        var parser = new FhirJsonParser();
        var bundle = parser.Parse<Bundle>(bundleJson);
        var documentReference = bundle.Entry.Select(entry => entry.Resource).OfType<DocumentReference>().First();

        var existingRole = documentReference.Contained.OfType<PractitionerRole>().First();
        var additionalRole = (PractitionerRole)existingRole.DeepCopy();
        additionalRole.Id = $"{existingRole.Id}-additional";
        additionalRole.Code =
        [
            new CodeableConcept
            {
                Coding =
                [
                    new Coding
                    {
                        System = "urn:oid:2.16.578.1.12.4.1.1.9060",
                        Code = "ADDITIONAL-ROLE",
                        Display = additionalRoleDisplay
                    }
                ]
            }
        ];

        documentReference.Contained.Add(additionalRole);
        documentReference.Author.Add(new ResourceReference($"#{additionalRole.Id}"));

        return documentReference;
    }

}