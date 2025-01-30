namespace XcaXds.Commons.Enums;

public enum StoredQuery
{
    FindDocuments,
    GetAll,
    GetDocuments,

    //Not supported by DIPS
    FindSubmissionSets,
    FindFolders,
    GetFolders,
    GetAssociations,
    GetDocumentsAndAssociations,
    GetSubmissionSets,
    GetSubmissionSetAndContents,
    GetFolderAndContents,
    GetFoldersForDocument,
    GetRelatedDocuments,
    FindDocumentsByReferenceId
}

//public enum DocumentType
//{
//    StableDocumentEntries,
//    OnDemandDocumentEntries
//}

//public enum AssociationType
//{
//    HasMember,
//    Replace,
//    Transformation,
//    Addendum,
//    ReplaceWithTransformation,
//    DigitalSignature,
//    SnapshotOfOnDemandDocumentEntry
//}

public enum ReturnType
{
    LeafClass,
    ObjectRef
}

public enum DocumentEntryQueryParameter
{
    PatientId,
    ClassCode,
    TypeCode,
    PracticeSettingCode,
    CreationTimeFrom,
    CreationTimeTo,
    ServiceStartTimeFrom,
    ServiceStartTimeTo,
    ServiceStopTimeFrom,
    ServiceStopTimeTo,
    HealthcareFacilityTypeCode,
    EventCodeList,
    ConfidentialityCode,
    AuthorPerson,
    FormatCode,
    Status,
    Type,
    UniqueId
}

public enum StatusValue
{
    Submitted,
    Approved,
    Deprecated
}

public enum SubmissionQueryParameter
{
    Status
}

public enum FolderQueryParameter
{
    Status
}

public enum GeneralQueryParameter
{
    PatientId
}

public enum PatientIdType
{
    Fodselsnummer,
    DNummer
}

/// <summary>
///     Based on HL7 Patient Identification Segment
///     http://www.hl7.eu/refactored/segPID.html
/// </summary>
public enum PidType
{
    /// <summary>
    /// Valuetype: string
    /// </summary>
    SetId = 1,

    /// <summary>
    /// Valuetype: Cx
    /// </summary>
    PatientIdentifierList = 3,

    /// <summary>
    /// Valuetype: Xpn
    /// </summary>
    PatientName = 5,

    /// <summary>
    /// Valuetype: Xpn
    /// </summary>
    MothersMaidenName = 6,

    /// <summary>
    /// Valuetype: DTM formatted string (yyyyMMddHHmmss)
    /// </summary>
    DateTimeofBirth = 7,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    AdministrativeSex = 8,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    Race = 10,

    /// <summary>
    /// Valuetype: Xad
    /// </summary>
    PatientAddress = 11,

    /// <summary>
    /// Valuetype: Xtn
    /// </summary>
    HomePhoneNumber = 13,

    /// <summary>
    /// Valuetype: Xtn
    /// </summary>
    BusinessPhoneNumber = 14,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    PrimaryLanguage = 15,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    MaritalStatus = 16,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    Religion = 17,

    /// <summary>
    /// Valuetype: Cx
    /// </summary>
    PatientAccountNumber = 18,

    /// <summary>
    /// Valuetype: Cx
    /// </summary>
    MothersIdentifier = 21,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    EthnicGroup = 22,

    /// <summary>
    /// Valuetype: string
    /// </summary>
    BirthPlace = 23,

    /// <summary>
    /// Valuetype: string
    /// </summary>
    MultipleBirthIndicator = 24,

    /// <summary>
    /// Valuetype: string
    /// </summary>
    BirthOrder = 25,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    Citizenship = 26,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    VeteransMilitaryStatus = 27,

    /// <summary>
    /// Valuetype: DTM formatted string (yyyyMMddHHmmss)
    /// </summary>
    PatientDeathDateandTime = 29,

    /// <summary>
    /// Valuetype: string
    /// </summary>
    PatientDeathIndicator = 30,

    /// <summary>
    /// Valuetype: string
    /// </summary>
    IdentityUnknownIndicator = 31,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    IdentityReliabilityCode = 32,

    /// <summary>
    /// Valuetype: DTM formatted string (yyyyMMddHHmmss)
    /// </summary>
    LastUpdateDateTime = 33,

    /// <summary>
    /// Valuetype: Hd
    /// </summary>
    LastUpdateFacility = 34,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    TaxonomicClassificationCode = 35,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    BreedCode = 36,

    /// <summary>
    /// Valuetype: string
    /// </summary>
    Strain = 37,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    ProductionClassCode = 38,

    /// <summary>
    /// Valuetype: Cwe
    /// </summary>
    TribalCitizenship = 39,

    /// <summary>
    /// Valuetype: Xtn
    /// </summary>
    PatientTelecommunicationInformation = 40
}

public enum ResponseStatusType
{
    Success,
    PartialSuccess,
    Failure
}

public enum ErrorSeverityType
{
    Warning,
    Error
}

public enum XdsErrorCodes
{
    /// <summary>
    /// A recipient queued the submission, for example, a manual process matching it to a patient.
    /// </summary>
    DocumentQueued,

    /// <summary>
    /// the recipient has rejected this submission because it detected that one of the documents does not match the metadata (e.g., formatcode) or has failed other requirements for the document content.
    /// when the registryerror element contains this error code, the codecontext shall contain the uniqueid of the document in error.
    /// if multiple documents are in error, there shall be a separate registryerror element for each document in error.
    /// </summary>
    InvalidDocumentContent,

    /// <summary>
    /// An XDR Document Recipient did not process some part of the content. Specifically, the parts not processed are Append semantics.
    /// </summary>
    PartialAppendContentNotProcessed,

    /// <summary>
    /// An XDR Document Recipient did not process some part of the content. Specifically, the parts not processed are Folder semantics.
    /// </summary>
    PartialFolderContentNotProcessed,

    /// <summary>
    /// An XDR Document Recipient did not process some part of the content. Specifically, the parts not processed are Relationship Association semantics.
    /// </summary>
    PartialRelationshipContentNotProcessed,

    /// <summary>
    /// An XDR Document Recipient did not process some part of the content. Specifically, the parts not processed are Replacement semantics.
    /// </summary>
    PartialReplaceContentNotProcessed,

    /// <summary>
    /// An XDR Document Recipient did not process some part of the content. Specifically, the parts not processed are Transform semantics.
    /// </summary>
    PartialTransformNotProcessed,

    /// <summary>
    /// An XDR Document Recipient did not process some part of the content. Specifically, the parts not processed are Transform and Replace semantics.
    /// </summary>
    PartialTransformReplaceNotProcessed,

    /// <summary>
    /// The recipient cannot resolve an entryUUID reference in the transaction.
    /// </summary>
    UnresolvedReferenceException,

    /// <summary>
    /// The document associated with the uniqueId is not available. This could be because the document is not available, the requestor is not authorized to access that document or the document is no longer available.
    /// </summary>
    XDSDocumentUniqueIdError,

    /// <summary>
    /// UniqueId received was not unique. UniqueId could have been attached to SubmissionSet or Folder.
    /// codeContext shall indicate which and the value of the non-unique uniqueId. This error cannot be thrown for DocumentEntry.
    /// </summary>
    XDSDuplicateUniqueIdInRegistry,

    /// <summary>
    /// This warning is returned if extra metadata was present but not saved.
    /// </summary>
    XDSExtraMetadataNotSaved,

    /// <summary>
    /// DocumentEntry exists in metadata with no matching Document element.
    /// </summary>
    XDSMissingDocument,

    /// <summary>
    /// Document element present with no matching DocumentEntry.
    /// </summary>
    XDSMissingDocumentMetadata,

    /// <summary>
    /// A value for the homeCommunityId is required and has not been specified.
    /// </summary>
    XDSMissingHomeCommunityId,

    /// <summary>
    /// Document being registered was a duplicate (uniqueId already in Document Registry) but hash does not match. The codeContext shall indicate uniqueId.
    /// </summary>
    XDSNonIdenticalHash,

    /// <summary>
    /// Document being registered was a duplicate (uniqueId already in Document Registry) but size does not match. The codeContext shall indicate uniqueId.
    /// </summary>
    XDSNonIdenticalSize,

    /// <summary>
    /// This error is thrown when the patient Id is required to match and does not. The codeContext shall indicate the value of the Patient Id and the nature of the conflict.
    /// </summary>
    XDSPatientIdDoesNotMatch,

    /// <summary>
    /// Too much activity.
    /// </summary>
    XDSRegistryBusy,
    XDSRepositoryBusy,

    /// <summary>
    /// The transaction was rejected because it submitted an Association referencing a deprecated document.
    /// </summary>
    XDSRegistryDeprecatedDocumentError,

    /// <summary>
    /// A uniqueId value was found to be used more than once within the submission. The errorCode indicates where the error was detected. The codeContext shall indicate the duplicate uniqueId.
    /// </summary>
    XDSRegistryDuplicateUniqueIdInMessage,
    XDSRepositoryDuplicateUniqueIdInMessage,

    /// <summary>
    /// Internal Error. 
    /// If one of these error codes is returned, the attribute codeContext shall contain details of the error condition that may be implementation-specific.
    /// </summary>
    XDSRegistryError,
    XDSRepositoryError,

    /// <summary>
    /// Error detected in metadata. Actor name indicates where error was detected. (Document Recipient uses Repository error). codeContext indicates nature of problem.
    /// </summary>
    XDSRegistryMetadataError,
    XDSRepositoryMetadataError,

    /// <summary>
    /// Repository was unable to access the Registry.
    /// </summary>
    XDSRegistryNotAvailable,

    /// <summary>
    /// System Resources are currently unavailable to respond to the request. The request may be retried later.
    /// </summary>
    XDSRegistryOutOfResources,
    XDSRepositoryOutOfResources,

    /// <summary>
    /// This error signals that a single request would have returned content for multiple Patient Ids.
    /// </summary>
    XDSResultNotSinglePatient,

    /// <summary>
    /// A required parameter to a stored query is missing.
    /// </summary>
    XDSStoredQueryMissingParam,

    /// <summary>
    /// A parameter which only accepts a single value is coded with multiple values.
    /// </summary>
    XDSStoredQueryParamNumber,

    /// <summary>
    /// The request cannot be satisfied due to being overly broad or having a response that is too large.
    /// The request should be adjusted, e.g., narrowed to reduce the number of results.
    /// </summary>
    XDSTooManyResults,

    /// <summary>
    /// A community which would have been contacted was not available.
    /// </summary>
    XDSUnavailableCommunity,

    /// <summary>
    /// A value for the homeCommunityId is not recognized.
    /// </summary>
    XDSUnknownCommunity,

    /// <summary>
    /// Patient Id referenced in the transaction is not known by the receiving actor. The codeContext shall include the value of patient Id in question.
    /// </summary>
    XDSUnknownPatientId,

    /// <summary>
    /// The repositoryUniqueId value could not be resolved to a valid document repository or the value does not match the repositoryUniqueId.
    /// </summary>
    XDSUnknownRepositoryId,

    /// <summary>
    /// The Query Id provided in the request is not recognized.
    /// </summary>
    XDSUnknownStoredQuery,

    /// <summary>
    /// An intendedRecipient which would have been contacted was not available.
    /// </summary>
    UnavailableRecipient,

    /// <summary>
    /// A value for intendedRecipient is not recognized.
    /// </summary>
    UnknownRecipient
}
