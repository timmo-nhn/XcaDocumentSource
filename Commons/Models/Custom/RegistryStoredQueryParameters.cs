using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models;

public static class RegistryStoredQueryParameters
{
    public static FindDocuments GetFindDocumentsParameters(AdhocQueryType adhocQuery)
    {
        return new FindDocuments()
        {
            XdsDocumentEntryPatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.PatientId).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.Status)?.GetValues(),
            XdsDocumentEntryClassCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.ClassCode).GetValues(),
            XdsDocumentEntryTypeCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.TypeCode).GetValues(),
            XdsDocumentEntryPracticeSettingCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.PracticeSettingCode).GetValues(),
            XdsDocumentEntryCreationTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.CreationTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryCreationTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.CreationTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStartTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.ServiceStartTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStartTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.ServiceStartTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStoptimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.ServiceStopTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStoptimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.ServiceStopTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryHealthcareFacilityTypeCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.HealthcareFacilityTypeCode).GetValues(),
            XdsDocumentEntryEventCodeList = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.EventCodeList).GetValues(),
            XdsDocumentEntryConfidentialityCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.ConfidentialityCode).GetValues(),
            XdsDocumentEntryAuthorPerson = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.AuthorPerson).GetValues(),
            XdsDocumentEntryFormatCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.FormatCode).GetValues(),
            XdsDocumentEntryType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.Type).GetValues(),
        };
    }
    
    public static FindSubmissionSets GetFindSubmissionSetsParameters(AdhocQueryType adhocQuery)
    {
        return new FindSubmissionSets()
        {
            XdsSubmissionSetPatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.PatientId).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetSourceId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.SourceId).GetValues(),
            XdsSubmissionSetSubmissionTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.SubmissionTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetSubmissionTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.SubmissionTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetAuthorPerson = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.AuthorPerson).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetContentType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.ContentType).GetValues(),
            XdsSubmissionSetStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Submission.Status).GetValues(),
        };
    }

    public static GetAll GetAllParameters(AdhocQueryType adhocQuery)
    {
        return new GetAll()
        {   
            PatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.PatientId).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryStatus).GetValues(),
            XdsSubmissionSetStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.SubmissionSetStatus).GetValues(),
            XdsFolderStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.FolderStatus).GetValues(),
            XdsDocumentEntryFormatCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryFormatCode).GetValues(),
            XdsDocumentEntryConfidentialityCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryConfidentialityCode).GetValues(),
            XdsDocumentEntryType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryType).GetValues(),
        };
    }
    
    public static GetDocuments GetDocumentsParameters(AdhocQueryType adhocQuery)
    {
        return new GetDocuments()
        {
            XdsDocumentEntryUuid = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetDocuments.XdsDocumentEntryUuid).GetValues(),
            XdsDocumentEntryUniqueId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetDocuments.XdsDocumentEntryUniqueId).GetValues(),
            HomeCommunityId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Associations.HomeCommunityId).FirstOrDefault()?.GetFirstValue(),
        };
    }  
    
    public static GetAssociations GetAssociationsParameters(AdhocQueryType adhocQuery)
    {
        return new GetAssociations()
        {
            Uuid = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Associations.Uuid).GetValues(),
            HomeCommunityId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Associations.HomeCommunityId).FirstOrDefault()?.GetFirstValue(),
        };
    }
}


// https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7
public class FindDocuments
{
    public string? XdsDocumentEntryPatientId { get; set; }
    public string[]? XdsDocumentEntryClassCode { get; set; }
    public string[]? XdsDocumentEntryTypeCode { get; set; }
    public string[]? XdsDocumentEntryPracticeSettingCode { get; set; }
    public string? XdsDocumentEntryCreationTimeFrom { get; set; }
    public string? XdsDocumentEntryCreationTimeTo { get; set; }
    public string? XdsDocumentEntryServiceStartTimeFrom { get; set; }
    public string? XdsDocumentEntryServiceStartTimeTo { get; set; }
    public string? XdsDocumentEntryServiceStoptimeFrom { get; set; }
    public string? XdsDocumentEntryServiceStoptimeTo { get; set; }
    public string[]? XdsDocumentEntryHealthcareFacilityTypeCode { get; set; }
    public string[]? XdsDocumentEntryEventCodeList { get; set; }
    public string[]? XdsDocumentEntryConfidentialityCode { get; set; }
    public string[]? XdsDocumentEntryAuthorPerson { get; set; }
    public string[]? XdsDocumentEntryFormatCode { get; set; }
    public string[]? XdsDocumentEntryStatus { get; set; }
    public string[]? XdsDocumentEntryType { get; set; }
}

public class FindSubmissionSets
{
    public string? XdsSubmissionSetPatientId { get; set; }
    public string[]? XdsSubmissionSetSourceId { get; set; }
    public string? XdsSubmissionSetSubmissionTimeFrom { get; set; }
    public string? XdsSubmissionSetSubmissionTimeTo { get; set; }
    public string? XdsSubmissionSetAuthorPerson { get; set; }
    public string[]? XdsSubmissionSetContentType { get; set; }
    public string[]? XdsSubmissionSetStatus { get; set; }
}

public class GetAll
{
    public string? PatientId { get; set; }
    public string[]? XdsDocumentEntryStatus { get; set; }
    public string[]? XdsSubmissionSetStatus { get; set; }
    public string[]? XdsFolderStatus { get; set; }
    public string[]? XdsDocumentEntryFormatCode { get; set; }
    public string[]? XdsDocumentEntryConfidentialityCode { get; set; }
    public string[]? XdsDocumentEntryType { get; set; }
}

public class GetDocuments
{
    public string[]? XdsDocumentEntryUuid { get; set; }
    public string[]? XdsDocumentEntryUniqueId { get; set; }
    public string? HomeCommunityId { get; set; }
}


public class GetAssociations
{
    public string[]? Uuid { get; set; }
    public string? HomeCommunityId { get; set; }

}
