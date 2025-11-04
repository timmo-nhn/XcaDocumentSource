using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public static class FhirTransformerService
{
    public static List<RegistryObjectDto> TransformFhirResourceToRegistryObjectDto(Bundle.EntryComponent bundleEntry)
    {
        var fhirDocumentReference = (DocumentReference)bundleEntry.Resource;

        var registryObjectList = new List<RegistryObjectDto>();

        var documentEntry =  new DocumentEntryDto();
        
        documentEntry.Id = bundleEntry.Resource.Id;
        documentEntry.Author = GetDocumentEntryAuthorsFromFhirDocumentReference(fhirDocumentReference);

        var submissionSet = new SubmissionSetDto();


        return registryObjectList;
    }

    private static List<AuthorInfo>? GetDocumentEntryAuthorsFromFhirDocumentReference(DocumentReference documentReference)
    {
        var author = new List<AuthorInfo>();



        return author;
    }
}

