using Grpc.Core;
using Hl7.Fhir.Model;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Tests.Helpers;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

public partial class IntegrationTests_XcaRespondingGatewayQueryRetrieve
{
    [Fact]
    public async Task PNR_UploadDocuments_RandomDocumentAmount()
    {
        _policyRepositoryService.DeleteAllPolicies();
        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        Assert.Equal(RegistryItemCount, _registry.DocumenRegistry.OfType<DocumentEntryDto>().Count());

        var metadata = TestHelpers.GenerateRegistryMetadata(RegistryItemCount,PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1,RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformerService.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti41-request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = documents;

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnR = RegistryItemCount + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Assert.Equal(expectedCountAfterPnR, _registry.DocumenRegistry.OfType<DocumentEntryDto>().Count());
        Assert.Equal(expectedCountAfterPnR, _repository.DocumentRepository.Count);
    }


    [Fact]
    public async Task PNR_UpdateMetadata()
    {
        _policyRepositoryService.DeleteAllPolicies();
        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var randomDocumentEntryToDeprecate = registryContent.PickRandom();


        var newDocumentEntry = TestHelpers.GenerateRegistryMetadata(1, PatientIdentifier.IdNumber, true).FirstOrDefault();

        var assocDto = new AssociationDto()
        {
            SourceObject = newDocumentEntry.DocumentEntry.Id,
            TargetObject = randomDocumentEntryToDeprecate.DocumentEntry.Id,
            AssociationType = Constants.Xds.AssociationType.Replace            
        };

        var submitObjectsUpdate = RegistryMetadataTransformerService.TransformRegistryObjectDtosToRegistryObjects([assocDto, newDocumentEntry.DocumentEntry, newDocumentEntry.Association, newDocumentEntry.SubmissionSet]);
        var documentUpdate = new DocumentType { Id = newDocumentEntry.Document.DocumentId, Value = newDocumentEntry.Document.Data };


        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti41-request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. submitObjectsUpdate];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = [documentUpdate];

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnrUpdate = RegistryItemCount + itemsToUploadCount;

        var iti42RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti42RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var statuses = _registry.DocumenRegistry.OfType<DocumentEntryDto>().Select(ro => ro.AvailabilityStatus).ToList();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterPnrUpdate, _registry.DocumenRegistry.OfType<DocumentEntryDto>().Count());
        Assert.Equal(expectedCountAfterPnrUpdate, _repository.DocumentRepository.Count);
        Assert.Equal(Constants.Xds.StatusValues.Deprecated, _registry.DocumenRegistry.OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.Id == randomDocumentEntryToDeprecate.DocumentEntry.Id)?.AvailabilityStatus);
    }


    [Fact]
    public async Task RDS_AddMetadata()
    {
        _policyRepositoryService.DeleteAllPolicies();
        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var metadata = TestHelpers.GenerateRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformerService.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();


        var iti42SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti42-request.xml"))));

        iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];

        var itemsToUploadCount = iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterRds = RegistryItemCount + itemsToUploadCount;

        var iti42RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti42SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti42RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterRds, _registry.DocumenRegistry.OfType<DocumentEntryDto>().Count());
        Assert.Equal(RegistryItemCount, _repository.DocumentRepository.Count);
    }


    [Fact]
    public async Task RMD_RemoveDocumentsAndMetadata()
    {
        _policyRepositoryService.DeleteAllPolicies();
        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_RemoveDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_QueryDocumentList",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_QueryDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti18-request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti43-request.xml"))));
        var iti62DeleteObjectsRequest = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti62-request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti86-request.xml"))));


        // Step -1: Pick random DocumentEntry to remove
        var documentEntryToRemove = RegistryContent.PickRandom().DocumentEntry;

        // Step 0: Check if Registry and Repository content is present
        Assert.Equal(RegistryItemCount, _registry.DocumenRegistry.OfType<DocumentEntryDto>().Count());

        // Step 1: Get the unique id for the DocumentEntry in the Registry...
        var iti18RmdRequest = new SoapEnvelope();
        iti18RmdRequest = iti18AdhocQuery; // Reusing this variable saves 0,000000124805 µg of CO2

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Slot = [new SlotType()
        {
            Name = Constants.Xds.QueryParameters.Associations.Uuid,
            ValueList = new() { Value = [documentEntryToRemove?.Id] }
        }];

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Id = Constants.Xds.StoredQueries.GetAssociations;

        var iti18RmdRequestSoapString = sxmls.SerializeSoapMessageToXmlString(iti18RmdRequest).Content;
        var iti18RmdRequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti18RmdRequestSoapString);

        var iti18RmdRequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti18RmdRequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));
        Assert.Equal(System.Net.HttpStatusCode.OK, iti18RmdRequestResponse.StatusCode);

        var iti18RmdRequestResponseContent = await iti18RmdRequestResponse.Content.ReadAsStringAsync();

        var iti18RmdResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti18RmdRequestResponseContent);

        var nogg =_registry.DocumenRegistry.OfType<AssociationDto>().FirstOrDefault(g => g.TargetObject == documentEntryToRemove.Id);

        Assert.Empty(iti18RmdResponseSoapObject?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

        var rmdAssociation = iti18RmdResponseSoapObject?.Body.AdhocQueryResponse?.RegistryObjectList.OfType<AssociationType>().FirstOrDefault(assoc => assoc.TargetObject == documentEntryToRemove?.Id);


        // Step 2: Use the identifiers in the Association to remove the metadata from the Registry...
        Assert.NotNull(rmdAssociation);
        iti62DeleteObjectsRequest.Body.RemoveObjectsRequest?.ObjectRefList?.ObjectRef = [
            new ObjectRefType(){Id = rmdAssociation.Id},
            new ObjectRefType(){Id = rmdAssociation.SourceObject},
            new ObjectRefType(){Id = rmdAssociation.TargetObject},
        ];

        var iti62RequestString = sxmls.SerializeSoapMessageToXmlString(iti62DeleteObjectsRequest).Content;

        var iti62RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti62RequestString);

        var iti62RequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti62RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));
        Assert.Equal(System.Net.HttpStatusCode.OK, iti62RequestResponse.StatusCode);

        var iti62ResponseContent = await iti62RequestResponse.Content.ReadAsStringAsync();

        var iti62ResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti62ResponseContent);
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);

        Assert.Equal(RegistryItemCount - 1, _registry.DocumenRegistry.OfType<DocumentEntryDto>().Count());


        // Step 3: Use the DocumentUniqueId in the DocumentEntry to remove the Document
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest?.DocumentRequest = new[]{

            new DocumentRequestType()
            {
                DocumentUniqueId = documentEntryToRemove?.UniqueId,
                HomeCommunityId = documentEntryToRemove?.HomeCommunityId,
                RepositoryUniqueId = documentEntryToRemove?.RepositoryUniqueId
            }
         };

        iti86DeleteDocumentSet.SetAction(Constants.Xds.OperationContract.Iti86Action);

        var iti86RequestString = sxmls.SerializeSoapMessageToXmlString(iti86DeleteDocumentSet).Content;

        var iti86RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti86RequestString);
        var iti86RequestResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti86RequestXmlDoc?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var iti86RequestResponseContent = await iti86RequestResponse.Content.ReadAsStringAsync();

        var iti86ResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti86RequestResponseContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, iti86RequestResponse.StatusCode);
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);
        Assert.Equal(RegistryItemCount - 1, _repository.DocumentRepository.Count);
    }


    [Fact]
    public async Task ALL_PutWrongRequestsForActions()
    {
        _policyRepositoryService.DeleteAllPolicies();
        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_RemoveDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_QueryDocumentList",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        AddAccessControlPolicyForIntegrationTest(
            policyName: "IT_QueryDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti18-request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti43-request.xml"))));
        var iti62DeleteObjects = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti62-request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti86-request.xml"))));

        iti18AdhocQuery.Body.RetrieveDocumentSetRequest = iti43RetrieveDocumentSet.Body.RetrieveDocumentSetRequest;
        iti43RetrieveDocumentSet.Body.AdhocQueryRequest = iti18AdhocQuery.Body.AdhocQueryRequest;
        iti18AdhocQuery.Body.AdhocQueryRequest = null;
        iti43RetrieveDocumentSet.Body.RetrieveDocumentSetRequest = null;

        iti62DeleteObjects.Body.RemoveDocumentsRequest = iti86DeleteDocumentSet.Body.RemoveDocumentsRequest;
        iti86DeleteDocumentSet.Body.RemoveObjectsRequest = iti62DeleteObjects.Body.RemoveObjectsRequest;
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest = null;
        iti62DeleteObjects.Body.RemoveObjectsRequest = null;

        var requests = new List<Dude>
        {
            new Dude { Request = iti18AdhocQuery, Endpoint = "/Registry/services/RegistryService"},
            new Dude { Request = iti62DeleteObjects, Endpoint = "/Registry/services/RegistryService"},
            new Dude { Request = iti86DeleteDocumentSet, Endpoint = "/Repository/services/RepositoryService"},
            new Dude { Request = iti43RetrieveDocumentSet, Endpoint = "/Repository/services/RepositoryService"}
        };

        foreach (var request in requests)
        {
            var soapRequestString = sxmls.SerializeSoapMessageToXmlString(request.Request).Content;
            var soapRequestResponse = await _client.PostAsync(request.Endpoint,
                new StringContent(GetSoapEnvelopeWithKjernejournalSamlToken(soapRequestString)?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

            var responseEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(await soapRequestResponse.Content.ReadAsStringAsync());
            Assert.NotNull(responseEnvelope.Body.Fault);
        }
    }
}

internal class Dude
{
    public SoapEnvelope Request { get; set; }
    public string Endpoint { get; set; }
}