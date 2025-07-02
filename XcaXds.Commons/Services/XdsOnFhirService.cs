
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public static class XdsOnFhirService
{
    public static AdhocQueryRequest GenerateIti18FromIti67(MhdDocumentRequest documentRequest)
    {
        var adhocQueryRequest = new AdhocQueryRequest();
        var adhocQuery = new AdhocQueryType();

        if (!string.IsNullOrWhiteSpace(documentRequest.Patient))
        {
            var patientCx = Hl7Object.Parse<CX>(documentRequest.Patient, '|');
            patientCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.Fnr
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId, [documentRequest.Patient]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.AuthorFamily))
        {
            adhocQuery.UpdateSlot(Constants.Xds.QueryParameters.FindDocuments.AuthorPerson, [documentRequest.AuthorFamily]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.AuthorGiven))
        {
            adhocQuery.UpdateSlot(Constants.Xds.QueryParameters.FindDocuments.AuthorPerson, [documentRequest.AuthorGiven]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Category))
        {
            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.ClassCode, [documentRequest.Category]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Event))
        {
            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.EventCodeList, [documentRequest.Event]);
        }

        adhocQueryRequest.AdhocQuery = adhocQuery;

        return adhocQueryRequest;
    }
}
