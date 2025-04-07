using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models;

public static class RegistryStoredQueryParameters
{
    public static FindDocuments GetFindDocumentsParameters(AdhocQueryType adhocQuery)
    {
        return new FindDocuments()
        {
            XdsDocumentEntryPatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.PatientId).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.Status)?.GetValuesGrouped(),
            XdsDocumentEntryClassCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.ClassCode).GetValuesGrouped(),
            XdsDocumentEntryTypeCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.TypeCode).GetValuesGrouped(),
            XdsDocumentEntryPracticeSettingCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.PracticeSettingCode).GetValuesGrouped(),
            XdsDocumentEntryCreationTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.CreationTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryCreationTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.CreationTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStartTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.ServiceStartTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStartTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.ServiceStartTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStoptimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.ServiceStopTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryServiceStoptimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.ServiceStopTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryHealthcareFacilityTypeCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.HealthcareFacilityTypeCode).GetValuesGrouped(),
            XdsDocumentEntryEventCodeList = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.EventCodeList).GetValuesGrouped(),
            XdsDocumentEntryConfidentialityCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.ConfidentialityCode).GetValuesGrouped(),
            XdsDocumentEntryAuthorPerson = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.AuthorPerson).GetValuesGrouped(),
            XdsDocumentEntryFormatCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.FormatCode).GetValuesGrouped(),
            XdsDocumentEntryType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindDocuments.Type).GetValuesGrouped(),
        };
    }

    public static FindSubmissionSets GetFindSubmissionSetsParameters(AdhocQueryType adhocQuery)
    {
        return new FindSubmissionSets()
        {
            XdsSubmissionSetPatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.PatientId).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetSourceId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.SourceId).GetValuesGrouped(),
            XdsSubmissionSetSubmissionTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.SubmissionTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetSubmissionTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.SubmissionTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetAuthorPerson = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.AuthorPerson).FirstOrDefault()?.GetFirstValue(),
            XdsSubmissionSetContentType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.ContentType).GetValuesGrouped(),
            XdsSubmissionSetStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindSubmissionSets.Status).GetValuesGrouped(),
        };
    }

    public static FindFolders GetFindFoldersParameters(AdhocQueryType adhocQuery)
    {
        return new FindFolders()
        {

            XdsFolderPatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindFoldes.XdsFolderPatientId).FirstOrDefault()?.GetFirstValue(),
            XdsFolderLastUpdateTimeFrom = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindFoldes.XdsFolderLastUpdateTimeFrom).FirstOrDefault()?.GetFirstValue(),
            XdsFolderLastUpdateTimeTo = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindFoldes.XdsFolderLastUpdateTimeTo).FirstOrDefault()?.GetFirstValue(),
            XdsFolderCodeList = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindFoldes.XdsFolderCodeList).GetValuesGrouped(),
            XdsFolderStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.FindFoldes.XdsFolderStatus).GetValuesGrouped(),
        };
    }

    public static GetAll GetAllParameters(AdhocQueryType adhocQuery)
    {
        return new GetAll()
        {
            PatientId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.PatientId).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryStatus).GetValuesGrouped(),
            XdsSubmissionSetStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.SubmissionSetStatus).GetValuesGrouped(),
            XdsFolderStatus = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.FolderStatus).GetValuesGrouped(),
            XdsDocumentEntryFormatCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryFormatCode).GetValuesGrouped(),
            XdsDocumentEntryConfidentialityCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryConfidentialityCode).GetValuesGrouped(),
            XdsDocumentEntryType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetAll.DocumentEntryType).GetValuesGrouped(),
        };
    }

    public static GetDocuments GetDocumentsParameters(AdhocQueryType adhocQuery)
    {
        return new GetDocuments()
        {
            XdsDocumentEntryUuid = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetDocuments.XdsDocumentEntryUuid).GetValuesGrouped(),
            XdsDocumentEntryUniqueId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetDocuments.XdsDocumentEntryUniqueId).GetValuesGrouped(),
            HomeCommunityId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Associations.HomeCommunityId).FirstOrDefault()?.GetFirstValue(),
        };
    }

    public static GetAssociations GetAssociationsParameters(AdhocQueryType adhocQuery)
    {
        return new GetAssociations()
        {
            Uuid = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Associations.Uuid).GetValuesGrouped(),
            HomeCommunityId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.Associations.HomeCommunityId).FirstOrDefault()?.GetFirstValue(),
        };
    }

    public static GetFolders GetFoldersParameters(AdhocQueryType adhocQuery)
    {
        return new GetFolders()
        {
            XdsFolderEntryUuid = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolders.XdsFolderEntryUuid).GetValuesGrouped(),
            XdsFolderUniqueId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolders.XdsFolderUniqueId).GetValuesGrouped()
        };
    }

    public static GetFolderAndContents GetFolderAndContentsParameters(AdhocQueryType adhocQuery)
    {
        return new GetFolderAndContents()
        {
            XdsFolderEntryUuid = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolderAndContents.XdsFolderEntryUuid).FirstOrDefault()?.GetFirstValue(),
            XdsFolderUniqueId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolderAndContents.XdsFolderUniqueId).FirstOrDefault()?.GetFirstValue(),
            XdsDocumentEntryFormatCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolderAndContents.XdsDocumentEntryFormatCode).GetValuesGrouped(),
            XdsDocumentEntryConfidentialityCode = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolderAndContents.XdsDocumentEntryConfidentialityCode).GetValuesGrouped(),
            XdsDocumentEntryType = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolderAndContents.XdsDocumentEntryType).GetValuesGrouped(),
            homeCommunityId = adhocQuery.GetSlot(Constants.Xds.QueryParamters.GetFolderAndContents.homeCommunityId).FirstOrDefault()?.GetFirstValue(),
        };
    }
}


// https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7
public class FindDocuments
{
    public string? XdsDocumentEntryPatientId { get; set; }
    public List<string[]>? XdsDocumentEntryClassCode { get; set; }
    public List<string[]>? XdsDocumentEntryTypeCode { get; set; }
    public List<string[]>? XdsDocumentEntryPracticeSettingCode { get; set; }
    public string? XdsDocumentEntryCreationTimeFrom { get; set; }
    public string? XdsDocumentEntryCreationTimeTo { get; set; }
    public string? XdsDocumentEntryServiceStartTimeFrom { get; set; }
    public string? XdsDocumentEntryServiceStartTimeTo { get; set; }
    public string? XdsDocumentEntryServiceStoptimeFrom { get; set; }
    public string? XdsDocumentEntryServiceStoptimeTo { get; set; }
    public List<string[]>? XdsDocumentEntryHealthcareFacilityTypeCode { get; set; }
    public List<string[]>? XdsDocumentEntryEventCodeList { get; set; }
    public List<string[]>? XdsDocumentEntryConfidentialityCode { get; set; }
    public List<string[]>? XdsDocumentEntryAuthorPerson { get; set; }
    public List<string[]>? XdsDocumentEntryFormatCode { get; set; }
    public List<string[]>? XdsDocumentEntryStatus { get; set; }
    public List<string[]>? XdsDocumentEntryType { get; set; }
}

public class FindSubmissionSets
{
    public string? XdsSubmissionSetPatientId { get; set; }
    public List<string[]> XdsSubmissionSetSourceId { get; set; }
    public string? XdsSubmissionSetSubmissionTimeFrom { get; set; }
    public string? XdsSubmissionSetSubmissionTimeTo { get; set; }
    public string? XdsSubmissionSetAuthorPerson { get; set; }
    public List<string[]> XdsSubmissionSetContentType { get; set; }
    public List<string[]> XdsSubmissionSetStatus { get; set; }
}

public class FindFolders
{
    public string? XdsFolderPatientId { get; set; }
    public string? XdsFolderLastUpdateTimeFrom { get; set; }
    public string? XdsFolderLastUpdateTimeTo { get; set; }
    public List<string[]>? XdsFolderCodeList { get; set; }
    public List<string[]>? XdsFolderStatus { get; set; }
}

public class GetAll
{
    public string? PatientId { get; set; }
    public List<string[]> XdsDocumentEntryStatus { get; set; }
    public List<string[]> XdsSubmissionSetStatus { get; set; }
    public List<string[]> XdsFolderStatus { get; set; }
    public List<string[]> XdsDocumentEntryFormatCode { get; set; }
    public List<string[]> XdsDocumentEntryConfidentialityCode { get; set; }
    public List<string[]> XdsDocumentEntryType { get; set; }
}

public class GetDocuments
{
    public List<string[]> XdsDocumentEntryUuid { get; set; }
    public List<string[]> XdsDocumentEntryUniqueId { get; set; }
    public string? HomeCommunityId { get; set; }
}


public class GetAssociations
{
    public List<string[]> Uuid { get; set; }
    public string? HomeCommunityId { get; set; }

}

public class GetFolders
{
    public List<string[]>? XdsFolderEntryUuid { get; set; }
    public List<string[]>? XdsFolderUniqueId { get; set; }
}

public class GetFolderAndContents
{
    public string? XdsFolderEntryUuid { get; set; }
    public string? XdsFolderUniqueId { get; set; }
    public List<string[]>? XdsDocumentEntryFormatCode { get; set; }
    public List<string[]>? XdsDocumentEntryConfidentialityCode { get; set; }
    public List<string[]>? XdsDocumentEntryType { get; set; }
    public string? homeCommunityId { get; set; }
}