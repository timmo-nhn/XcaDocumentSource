using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using XcaXds.Shared.Models.Custom;

namespace XcaXds.Shared;

public static class Constants
{
    public static class Properties
    {
        public const int MaxStringLength = 255;
        public const int MaxArrayLength = 65535;
    }
    public static class Oid
    {
        // The "system"-value for OID
        public const string System = "urn:ietf:rfc:3986";
    }

    public static class FileSizes
    {
        public const long OneMb = 1L * 1024 * 1024;
        public const long FiftyMb = 50L * 1024 * 1024;
        public const long OneHundredMb = 100L * 1024 * 1024;
        public const long OneGb = 1L * 1024 * 1024 * 1024;
    }

    public static class Scopes
    {
        public static class FhirMobileAccessToHealthDocuments
        {
            // These scopes must match scopes in Norsk helsenett dokumentlager in Selvbetjening
            public const string ScopeCreateDocuments = "nhn:phr-repo/mhd/create";
            public const string ScopeDeleteDocument = "nhn:phr-repo/delete-docs-and-refs";

            // Old values
            //public const string ScopeCreateDocuments = "nhn:phr-document-repository/mhd/create-documents-with-reference";
            //public const string ScopeCreateDocuments = "nhn:phr-document-repository/delete-documents-and-reference";
        }
    }

    public static class XmlDefaultOptions
    {
        public static readonly XmlWriterSettings DefaultXmlWriterSettings = new()
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true,
        };

        public static readonly XmlWriterSettings DefaultXmlWriterSettingsInline = new()
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true,
        };
    }

    public static class JsonDefaultOptions
    {
        public static readonly JsonSerializerOptions DefaultSettings = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            },
        };

        public static readonly JsonSerializerOptions DefaultSettingsInline = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };
    }


    public static class Soap
    {
        public static class Addresses
        {
            public const string Anonymous = "http://www.w3.org/2005/08/addressing/anonymous";
        }

        public static class Namespaces
        {
            public const string SoapEnvelope = "http://www.w3.org/2003/05/soap-envelope";
            public const string Addressing = "http://www.w3.org/2005/08/addressing";
            public const string AddressingSoapFault = "http://www.w3.org/2005/08/addressing/soap/fault";
            public const string SecurityUtility = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";
            public const string Xsd = "http://www.w3.org/2001/XMLSchema";
            public const string SecurityExt = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
            public const string Saml2 = "urn:oasis:names:tc:SAML:2.0:assertion";
            public const string Svs = "urn:ihe:iti:svs:2008";
            public const string Xdsb = "urn:ihe:iti:xds-b:2007";
            public const string Query = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0";
            public const string Rs = "urn:oasis:names:tc:ebxml-regrep:xsd:rs:3.0";
            public const string Rim = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0";
            public const string Lcm = "urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0";
            public const string Hl7V3 = "urn:hl7-org:v3";
            public const string XopInclude = "http://www.w3.org/2004/08/xop/include";
        }
    }

    public static class Xds
    {
        public static class Namespaces
        {
            public const string Xdsb = "urn:ihe:iti:xds-b:2007";
            public const string Query = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0";
            public const string Rs = "urn:oasis:names:tc:ebxml-regrep:xsd:rs:3.0";
            public const string Rim = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0";
            public const string Lcm = "urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0";
            public const string Xsd = "urn:oasis:names:tc:ebxml-regrep:xsd:3.0";
            public const string Svs = "urn:ihe:iti:svs:2008";
            public const string Hl7V3 = "urn:hl7-org:v3";
            public const string Rmd = "urn:ihe:iti:rmd:2017";
        }

        public static class OperationContract
        {
            /// <summary>
            /// Root OID for the ITI Domain (ITI-messages)
            /// </summary>
            public const string System = "urn:ihe:event-type-code";

            public const string Iti18Action = "urn:ihe:iti:2007:RegistryStoredQuery";
            public const string Iti18ActionAsync = "urn:ihe:iti:2007:RegistryStoredQueryAsync";
            public const string Iti18Reply = "urn:ihe:iti:2007:RegistryStoredQueryResponse";
            public const string Iti18ReplyAsync = "urn:ihe:iti:2007:RegistryStoredQueryResponseAsync";
            public const string Iti38Action = "urn:ihe:iti:2007:CrossGatewayQuery";
            public const string Iti38ActionAsync = "urn:ihe:iti:2007:CrossGatewayQueryAsync";
            public const string Iti38Reply = "urn:ihe:iti:2007:CrossGatewayQueryResponse";
            public const string Iti38ReplyAsync = "urn:ihe:iti:2007:CrossGatewayQueryResponseAsync";
            public const string Iti39Action = "urn:ihe:iti:2007:CrossGatewayRetrieve";
            public const string Iti39ActionAsync = "urn:ihe:iti:2007:CrossGatewayRetrieveAsync";
            public const string Iti39Reply = "urn:ihe:iti:2007:CrossGatewayRetrieveResponse";
            public const string Iti39ReplyAsync = "urn:ihe:iti:2007:CrossGatewayRetrieveResponseAsync";
            public const string Iti43Action = "urn:ihe:iti:2007:RetrieveDocumentSet";
            public const string Iti43ActionAsync = "urn:ihe:iti:2007:RetrieveDocumentSetAsync";
            public const string Iti43Reply = "urn:ihe:iti:2007:RetrieveDocumentSetResponse";
            public const string Iti43ReplyAsync = "urn:ihe:iti:2007:RetrieveDocumentSetResponseAsync";
            public const string Iti41Action = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-b";
            public const string Iti41ActionAsync = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-bAsync";
            public const string Iti41Reply = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-bResponse";
            public const string Iti41ReplyAsync = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-bResponseAsync";
            public const string Iti42Action = "urn:ihe:iti:2007:RegisterDocumentSet-b";
            public const string Iti42ActionAsync = "urn:ihe:iti:2007:RegisterDocumentSet-bAsync";
            public const string Iti42Reply = "urn:ihe:iti:2007:RegisterDocumentSet-bResponse";
            public const string Iti42ReplyAsync = "urn:ihe:iti:2007:RegisterDocumentSet-bResponseAsync";
            public const string Iti62Action = "urn:ihe:iti:2010:DeleteDocumentSet";
            public const string Iti62ActionAsync = "urn:ihe:iti:2010:DeleteDocumentSetAsync";
            public const string Iti62Reply = "urn:ihe:iti:2010:DeleteDocumentSetResponse";
            public const string Iti62ReplyAsync = "urn:ihe:iti:2010:DeleteDocumentSetResponseAsync";
            public const string Iti86Action = "urn:ihe:iti:2017:RemoveDocuments";
            public const string Iti86ActionAsync = "urn:ihe:iti:2017:RemoveDocumentsAsync";
            public const string Iti86Reply = "urn:ihe:iti:2017:RemoveDocumentsResponse";
            public const string Iti86ReplyAsync = "urn:ihe:iti:2017:RemoveDocumentsResponseAsync";
        }

        public static class StoredQueries
        {
            public const string FindDocuments = "urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d";
            public const string FindSubmissionSets = "urn:uuid:f26abbcb-ac74-4422-8a30-edb644bbc1a9";
            public const string FindFolders = "urn:uuid:958f3006-baad-4929-a4de-ff1114824431";
            public const string GetAssociations = "urn:uuid:a7ae438b-4bc2-4642-93e9-be891f7bb155";
            public const string GetFolders = "urn:uuid:5737b14c-8a1a-4539-b659-e03a34a5e1e4";
            public const string GetFolderAndContents = "urn:uuid:b909a503-523d-4517-8acf-8e5834dfc4c7";

            //Not natively supported by XcaDocumentSource

            //public const string GetAll = "urn:uuid:10b545ea-725c-446d-9b95-8aeb444eddf3";
            //public const string GetDocuments = "urn:uuid:5c4f972b-d56b-40ac-a5fc-c8ca9b40b9d4";
            //public const string GetDocumentsAndAssociations = "urn:uuid:bab9529a-4a10-40b3-a01f-f68a615d247a";
            //public const string GetSubmissionSets = "urn:uuid:51224314-5390-4169-9b91-b1980040715a";
            //public const string GetSubmissionSetAndContents = "urn:uuid:e8e3cb2c-e39c-46b9-99e4-c12f57260b83";
            //public const string GetFoldersForDocument = "urn:uuid:10cae35a-c7f9-4cf5-b61e-fc3278ffb578";
            //public const string GetRelatedDocuments = "urn:uuid:d90e5407-b356-4d91-a89f-873917b4b0e6";
            //public const string FindDocumentsByReferenceId = "urn:uuid:12941a89-e02e-4be5-967c-ce4bfc8fe492";
        }

        public static class QueryParameters
        {
            public static class FindDocuments
            {
                public const string PatientId = "$XDSDocumentEntryPatientId";
                public const string ClassCode = "$XDSDocumentEntryClassCode";
                public const string TypeCode = "$XDSDocumentEntryTypeCode";
                public const string PracticeSettingCode = "$XDSDocumentEntryPracticeSettingCode";
                public const string CreationTimeFrom = "$XDSDocumentEntryCreationTimeFrom";
                public const string CreationTimeTo = "$XDSDocumentEntryCreationTimeTo";
                public const string ServiceStartTimeFrom = "$XDSDocumentEntryServiceStartTimeFrom";
                public const string ServiceStartTimeTo = "$XDSDocumentEntryServiceStartTimeTo";
                public const string ServiceStopTimeFrom = "$XDSDocumentEntryServiceStopTimeFrom";
                public const string ServiceStopTimeTo = "$XDSDocumentEntryServiceStopTimeTo";
                public const string HealthcareFacilityTypeCode = "$XDSDocumentEntryHealthcareFacilityTypeCode";
                public const string EventCodeList = "$XDSDocumentEntryEventCodeList";
                public const string ConfidentialityCode = "$XDSDocumentEntryConfidentialityCode";
                public const string AuthorPerson = "$XDSDocumentEntryAuthorPerson";
                public const string FormatCode = "$XDSDocumentEntryFormatCode";
                public const string Status = "$XDSDocumentEntryStatus";
                public const string Type = "$XDSDocumentEntryType";
                public const string EntryUuid = "$XDSDocumentEntryEntryUUID";
                public const string UniqueId = "$XDSDocumentEntryUniqueId";
            }

            public static class FindSubmissionSets
            {
                public const string PatientId = "$XDSSubmissionSetPatientId";
                public const string SourceId = "$XDSSubmissionSetSourceId";
                public const string SubmissionTimeFrom = "$XDSSubmissionSetSubmissionTimeFrom";
                public const string SubmissionTimeTo = "$XDSSubmissionSetSubmissionTimeTo";
                public const string AuthorPerson = "$XDSSubmissionSetAuthorPerson";
                public const string ContentType = "$XDSSubmissionSetContentType";
                public const string Status = "$XDSSubmissionSetStatus";
            }

            public static class Folder
            {
                public const string Status = "$XDSFolderStatus";
            }

            public static class GetFolders
            {
                public const string XdsFolderEntryUuid = "$XDSFolderEntryUUID";
                public const string XdsFolderUniqueId = "$XDSFolderUniqueId";
            }

            public static class FindFoldes
            {
                public const string XdsFolderPatientId = "$XDSFolderPatientId";
                public const string XdsFolderLastUpdateTimeFrom = "$XDSFolderLastUpdateTimeFrom";
                public const string XdsFolderLastUpdateTimeTo = "$XDSFolderLastUpdateTimeTo";
                public const string XdsFolderCodeList = "$XDSFolderCodeList";
                public const string XdsFolderStatus = "$XDSFolderStatus";

            }

            public static class GetFolderAndContents
            {
                public const string XdsFolderEntryUuid = "$XDSFolderEntryUUID";
                public const string XdsFolderUniqueId = "$XDSFolderUniqueId";
                public const string XdsDocumentEntryFormatCode = "$XDSDocumentEntryFormatCode";
                public const string XdsDocumentEntryConfidentialityCode = "$XDSDocumentEntryConfidentialityCode";
                public const string XdsDocumentEntryType = "$XDSDocumentEntryType";
                public const string homeCommunityId = "$XDSDocumentEntryFormatCode";
            }

            public static class Associations
            {
                public const string Uuid = "$uuid";
                public const string HomeCommunityId = "$homeCommunityId";
            }

            public static class General
            {
                public const string PatientId = "$patientId";
            }

            public static class GetAll
            {
                public const string PatientId = "$patientId";
                public const string DocumentEntryStatus = "$XDSDocumentEntryStatus";
                public const string SubmissionSetStatus = "$XDSSubmissionSetStatus";
                public const string FolderStatus = "$XDSFolderStatus";
                public const string DocumentEntryFormatCode = "$XDSDocumentEntryFormatCode";
                public const string DocumentEntryConfidentialityCode = "$XDSDocumentEntryConfidentialityCode";
                public const string DocumentEntryType = "$XDSDocumentEntryType";
                public const string HomeCommunityId = "$homeCommunityId";
            }

            public static class GetDocuments
            {
                public const string XdsDocumentEntryUuid = "$XDSDocumentEntryEntryUUID";
                public const string XdsDocumentEntryUniqueId = "$XDSDocumentEntryUniqueId";
            }
        }

        public static class StatusValues
        {
            public const string Submitted = "urn:oasis:names:tc:ebxml-regrep:StatusType:Submitted";
            public const string Approved = "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved";
            public const string Deprecated = "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated";
        }

        public static class ReturnType
        {
            public const string LeafClass = "LeafClass";
            public const string ObjectRef = "ObjectRef";
        }

        public static class ResponseStatusTypes
        {
            public const string Failure = "urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Failure";
            public const string Success = "urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Success";
            public const string PartialSuccess = "urn:ihe:iti:2007:ResponseStatusType:PartialSuccess";
        }

        public static class ErrorSeverity
        {
            public const string Warning = "urn:oasis:names:tc:ebxml-regrep:ErrorSeverityType:Warning";
            public const string Error = "urn:oasis:names:tc:ebxml-regrep:ErrorSeverityType:Error";
        }

        public static class Uuids
        {
            public static class SubmissionSet
            {
                public const string SubmissionSetClassificationNode = "urn:uuid:a54d6aa5-d40d-43f9-88c5-b4633d873bdd";
                public const string Author = "urn:uuid:a7058bb9-b4e4-4307-ba5b-e3f0ab85e12d";
                public const string ContentTypeCode = "urn:uuid:aa543740-bdda-424e-8c96-df4873be8500";
                public const string PatientId = "urn:uuid:6b5aea1a-874d-4603-a4bc-96a0a7b38446";
                public const string SourceId = "urn:uuid:554ac39e-e3fe-47fe-b233-965d2a147832";
                public const string UniqueId = "urn:uuid:96fdda7c-d067-4183-912e-bf5ee74998a8";
            }

            public static class DocumentEntry
            {
                public const string StableDocumentEntries = "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1";
                public const string OnDemandDocumentEntries = "urn:uuid:34268e47-fdf5-41a6-ba33-82133c465248";
                public const string Author = "urn:uuid:93606bcf-9494-43ec-9b4e-a7748d1a838d";
                public const string ClassCode = "urn:uuid:41a5887f-8865-4c09-adf7-e362475b143a";
                public const string ConfidentialityCode = "urn:uuid:f4f85eac-e6cb-4883-b524-f2705394840f";
                public const string EventCodeList = "urn:uuid:2c6b8cb7-8b2a-4051-b291-b1ae6a575ef4";
                public const string FormatCode = "urn:uuid:a09d5840-386c-46f2-b5ad-9c3699a4309d";
                public const string HealthCareFacilityTypeCode = "urn:uuid:f33fb8ac-18af-42cc-ae0e-ed0b0bdb91e1";
                public const string PatientId = "urn:uuid:58a6f841-87b3-4a3e-92fd-a8ffeff98427";
                public const string PracticeSettingCode = "urn:uuid:cccf5598-8b07-4b77-a05e-ae952c785ead";
                public const string TypeCode = "urn:uuid:f0306f51-975f-434e-a61c-c59651d33983";
                public const string UniqueId = "urn:uuid:2e82c1f6-a085-4c72-9da3-8640a32e42ab";
                public const string ReferenceIdList = "urn:ihe:iti:xds:2013:referenceIdList";
            }

            public static class Folder
            {
                public const string FolderClassificationNode = "urn:uuid:d9d542f3-6cc4-48b6-8870-ea235fbc94c2";
                public const string CodeList = "urn:uuid:1ba97051-7806-41a8-a48b-8fce7af683c5";
                public const string PatientId = "urn:uuid:f64ffdf0-4b97-4e06-b79f-a52b38ec2f8a";
                public const string UniqueId = "urn:uuid:75df8f67-9973-4fbe-a900-df66cefecc5a";
                public const string Association = "urn:uuid:abd807a3-4432-4053-87b4-fd82c643d1f3";
            }
        }

        public static class ObjectTypes
        {
            public const string Classification = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification";
            public const string Association = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Association";
            public const string RegistryPackage = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:RegistryPackage";
            public const string ExternalIdentifier = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier";
        }

        public static class AssociationType
        {
            public const string HasMember = "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember";
            public const string Replace = "urn:ihe:iti:2007:AssociationType:RPLC";
            public const string Transformation = "urn:ihe:iti:2007:AssociationType:XFRM";
            public const string Addendum = "urn:ihe:iti:2007:AssociationType:APND";
            public const string ReplaceWithTransformation = "urn:ihe:iti:2007:AssociationType:XFRM_RPLC";
            public const string DigitalSignature = "urn:ihe:iti:2007:AssociationType:signs";
            public const string SnapshotOfOnDemandDocumentEntry = "urn:ihe:iti:2010:AssociationType:IsSnapshotOf";
        }

        public static class ExternalIdentifierNames
        {
            public const string SubmissionSetPatientId = "XDSSubmissionSet.patientId";
            public const string SubmissionSetSourceId = "XDSSubmissionSet.sourceId";
            public const string SubmissionSetUniqueId = "XDSSubmissionSet.uniqueId";
            public const string DocumentEntryPatientId = "XDSDocumentEntry.patientId";
            public const string DocumentEntryUniqueId = "XDSDocumentEntry.uniqueId";
        }

        public static class ClassificationNames
        {
            public const string Author = "author";
            public const string SubmissionSetAuthor = "XDSSubmissionSet.author";
        }

        public static class SlotNames
        {
            public const string AuthorRole = "authorRole";
            public const string AuthorPerson = "authorPerson";
            public const string AuthorSpecialty = "authorSpecialty";
            public const string AuthorInstitution = "authorInstitution";
            public const string AuthorTelecommunication = "authorTelecommunication";
            public const string CreationTime = "creationTime";
            public const string HomeCommunityId = "homeCommunityId";
            public const string LanguageCode = "languageCode";
            public const string LegalAuthenticator = "legalAuthenticator";
            public const string CodingScheme = "codingScheme";
            public const string SubmissionSetStatus = "SubmissionSetStatus";
            public const string PreviousVersion = "PreviousVersion";
            public const string SubmissionTime = "submissionTime";
            public const string IntendedRecipient = "intendedRecipient";
            public const string SourcePatientInfo = "sourcePatientInfo";
            public const string Size = "size";
            public const string Hash = "hash";
            public const string RepositoryUniqueId = "repositoryUniqueId";
            public const string ServiceStartTime = "serviceStartTime";
            public const string ServiceStopTime = "serviceStopTime";
            public const string SourcePatientId = "sourcePatientId";
        }

        public static class ErrorCodes
        {
            public const string XdsUnavailableCommunity = "XDSUnavailableCommunity";
            public const string XdsRepositoryError = "XDSRepositoryError";
            public const string XdsRegistryError = "XDSRegistryError";
            public const string XdsRepositoryBusy = "XDSRepositoryBusy";
            public const string XdsRegistryBusy = "XDSRegistryBusy";
            public const string XdsDocumentUniqueIdError = "XDSDocumentUniqueIdError";
            public const string XdsMetadataUpdateError = "XDSMetadataUpdateError";

            public const string XdsRepresentationBelowMinimumAgeError = "XDSRepresentationBelowMinimumAgeError";
        }
    }

    public static class Hl7
    {
        public static class StructureDefinition
        {
            public const string MinimalProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Minimal.ProvideBundle";
            public const string UncontainedComprehensiveProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.UnContained.Comprehensive.ProvideBundle";
            public const string ComprehensiveProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.ProvideBundle";
            public const string IheHomeCommunityId = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId";
        }

        public static class Dtm
        {
            /// <summary>
            /// yyyy
            /// </summary>
            public const string DtmYFormat = "yyyy";

            /// <summary>
            /// yyyyMM
            /// </summary>
            public const string DtmYmFormat = DtmYFormat + "MM";

            /// <summary>
            /// yyyyMMdd
            /// </summary>
            public const string DtmYmdFormat = DtmYmFormat + "dd";

            /// <summary>
            /// yyyyMMddHH
            /// </summary>
            public const string DtmYmdhFormat = DtmYmdFormat + "HH";

            /// <summary>
            /// yyyyMMddHHmm
            /// </summary>
            public const string DtmYmdhmFormat = DtmYmdhFormat + "mm";

            /// <summary>
            /// yyyyMMddHHmmss
            /// </summary>
            public const string DtmFormat = DtmYmdhmFormat + "ss";

            /// <summary>
            /// yyMMddHHmmss
            /// </summary>
            public const string DtmYyFormat = "yyMMddHHmmss";

            /// <summary>
            /// yyyyMMddHHmmssfff
            /// </summary>
            public const string DtmLongFormat = DtmFormat + "fff";

            public const string DtmFhirIsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ssK";

            public const string DtmFhirIsoDateFormat = "yyyy-MM-dd";

            public static readonly string[] AllFormats =
            [
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyyMMddHHmmss.FFFFzzz",
                "yyyyMMddHHmmss.FFFF",
                "yyyyMMddHHmmsszzz",
                "yyyyMMddHHmmss",
                "yyyyMMddHHmm.FFFFzzz",
                "yyyyMMddHHmm.FFFF",
                "yyyyMMddHHmmzzz",
                "yyyyMMdd",
                "yyyyddMM",
                "yyyyMM",
                "yyyy"
            ];

        }

        public static class Namespaces
        {
            public const string Hl7V3 = "urn:hl7-org:v3";
            public const string Hl7Sdtc = "urn:hl7-org:sdtc";
        }

        public static class Separator
        {
            public const char Ampersand = '&';
            public const char Caret = '^';
        }

        public static class UniversalIdType
        {
            //http://www.hl7.eu/refactored/tab0301.html
            public const string Iso = "ISO";

            public const string Uuid = "UUID";
            public const string Guid = "GUID";
            public const string Dns = "DNS";
        }

        public static class Pid
        {
            public const string PidBase = "PID-";
        }
    }

    public static class MimeTypes
    {
        public const string Acc = "audio/aac";
        public const string Avi = "video/x-msvideo";
        public const string Binary = "application/octet-stream";
        public const string Doc = "application/msword";
        public const string Docm = "application/vnd.ms-word.document.macroEnabled.12";
        public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string Exe = "application/vnd.microsoft.portable-executable";
        public const string FhirJson = "application/fhir+json";
        public const string Gif = "image/gif";
        public const string Hl7v3Xml = "application/hl7-v3+xml";
        public const string Html = "text/html";
        public const string Jpeg = "image/jpeg";
        public const string Json = "application/json";
        public const string Mp4 = "video/mp4";
        public const string Mpeg = "video/mpeg";
        public const string MultipartRelated = "multipart/related";
        public const string Odp = "application/vnd.oasis.opendocument.presentation";
        public const string Ods = "application/vnd.oasis.opendocument.spreadsheet";
        public const string Odt = "application/vnd.oasis.opendocument.text";
        public const string Oga = "audio/ogg";
        public const string Ogv = "video/ogg";
        public const string Pdf = "application/pdf";
        public const string Png = "image/png";
        public const string Pps = "application/vnd.ms-powerpoint";
        public const string Ppsm = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
        public const string Ppt = "application/vnd.ms-powerpoint";
        public const string Pptm = "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
        public const string Pptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public const string Rtf = "application/rtf";
        public const string TextRtf = "text/rtf";
        public const string SevenZip = "application/x-7z-compressed";
        public const string SoapXml = "application/soap+xml";
        public const string Text = "text/plain";
        public const string Tiff = "image/tiff";
        public const string Vsd = "application/vnd.visio";
        public const string Wav = "audio/x-wav";
        public const string Weba = "audio/webm";
        public const string Webm = "video/webm";
        public const string Webp = "image/webp";
        public const string Xhtml = "application/xhtml+xml";
        public const string Xls = "application/vnd.ms-excel";
        public const string Xlsb = "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
        public const string Xlsm = "application/vnd.ms-excel.sheet.macroEnabled.12";
        public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Xml = "application/xml";
        public const string XmlReadable = "text/xml";
        public const string XopXml = "application/xop+xml";
        public const string Zip = "application/zip";
    }

    public static class Xacml
    {
        public static class Actions
        {
            public const string Create = "Create";
            public const string ReadDocumentList = "ReadDocumentList";
            public const string ReadDocuments = "ReadDocuments";
            public const string Update = "Update";
            public const string Delete = "Delete";
            public const string Execute = "Execute";
            public const string Unknown = "Unknown";
        }

        public static class Attribute
        {
            public const string ActionId = "urn:oasis:names:tc:xacml:1.0:action:action-id";
            public const string SubjectId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
            public const string ResourceId = "urn:oasis:names:tc:xacml:2.0:resource:resource-id";
            public const string Role = "urn:oasis:names:tc:xspa:1.0:subject:role";
        }
    }

    public static class Saml
    {
        public static class Attribute
        {
            // --- XSPA core subject attributes ---
            public const string SubjectId = "urn:oasis:names:tc:xspa:1.0:subject:subject-id";
            public const string Organization = "urn:oasis:names:tc:xspa:1.0:subject:organization";
            public const string OrganizationId = "urn:oasis:names:tc:xspa:1.0:subject:organization-id";
            public const string ChildOrganization = "urn:oasis:names:tc:xspa:1.0:subject:child-organization";
            public const string Role = "urn:oasis:names:tc:xspa:1.0:subject:role";
            public const string PurposeOfUse = "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse";
            public const string PurposeOfUse_Helsenorge = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse";
            public const string Npi = "urn:oasis:names:tc:xspa:2.0:subject:npi";

            // --- IHE / XUA / XCA / BPPC ---
            public const string HomeCommunityIdXca = "urn:ihe:iti:xca:2010:homeCommunityId";
            public const string ProviderIdentifier = "urn:ihe:iti:xua:2017:subject:provider-identifier";
            public const string BppcDocId = "urn:ihe:iti:bppc:2007:docid";
            public const string XuaAcp = "urn:ihe:iti:xua:2012:acp";

            // --- XACML attributes ---
            public const string ResourceId10 = "urn:oasis:names:tc:xacml:1.0:resource:resource-id";
            public const string ResourceId20 = "urn:oasis:names:tc:xacml:2.0:resource:resource-id";
            public const string SubjectRole20 = "urn:oasis:names:tc:xacml:2.0:subject:role";
            public const string ActionPurpose20 = "urn:oasis:names:tc:xacml:2.0:action:purpose";

            // --- eHelse-specific attributes ---
            public const string EhelseHomeCommunityId = "urn:no:ehelse:saml:1.0:subject:homeCommunityId";
            public const string EhelseSecurityLevel = "urn:no:ehelse:saml:1.0:subject:SecurityLevel";
            public const string EhelseScope = "urn:no:ehelse:saml:1.0:subject:Scope";
            public const string EhelseClientId = "urn:no:ehelse:saml:1.0:subject:client_id";
            public const string EhelseAuthenticationMethod = "urn:no:ehelse:saml:1.0:subject:Authentication_method";
            public const string EhelseHealthcareService = "urn:no:ehelse:saml:1.1:subject:healthcareservice";

            // --- NHN Trust Framework extensions ---
            public const string TrustChildOrgName = "urn:nhn:trust-framework:1.0:ext:subject:child-organization-name";
            public const string TrustResourceChildOrg = "urn:nhn:trust-framework:1.0:ext:resource:child-organization";
            public const string TrustResourceChildOrgId = "urn:nhn:trust-framework:1.0:ext:resource:child-organization-id";
            public const string TrustHealthcareService = "urn:nhn:trust-framework:1.0:ext:care-relationship:healthcare-service";
            public const string TrustPurposeOfUseDetails = "urn:nhn:trust-framework:1.0:ext:care-relationship:purpose-of-use-details";
            public const string TrustDecisionRef = "urn:nhn:trust-framework:1.0:ext:care-relationship:decision-ref";

            // --- Generic / misc ---
            public const string SamlSubjectId = "urn:oasis:names:tc:SAML:attribute:subject-id";
        }
    }

    public static class JwtSaml
    {
        public const string SecurityLevelClaimType = "helseid://claims/identity/security_level";
        public const string TillitsrammeverkClaimType = "nhn:tillitsrammeverk:parameters";
        public const string FastlegeClaimType = "fastlege";
    }

    public static class Urn
    {
        public static class Custom
        {
            public const string BaseUrn = "urn:xcads";
            public const string DocumentEntryPatientIdentifier = BaseUrn + ":document:patient-identifier";
            public const string AdhocQueryPatientIdentifier = BaseUrn + ":adhocquery:patient-identifier";
            public const string DocumentUniqueId = BaseUrn + ":document:uniqueid";
            public const string RepositoryUniqueId = BaseUrn + ":document:repositoryuniqueid";
            public const string HomeCommunityId = BaseUrn + ":document:homecommunityid";
            public const string SamlAttributes = BaseUrn + ":saml:attributes";
            public const string SamlNameId = BaseUrn + SamlAttributes + ":nameid";
            public const string AppliesTo = BaseUrn + ":xacml:appliesto";
            public const string UnknownAttribute = BaseUrn + ":xacml:unknownattribute";
            public const string UnknownPatientIdentifier = BaseUrn + ":unknown-patient-identifier";
        }
    }
}

public static class ConstantsExtensions
{
    public static Dictionary<string, string> GetAsDictionary(this Type type)
    {
        var constants = new Dictionary<string, string>();

        // Get all static fields of the class
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var value = (string?)field.GetValue(null);
            // Ensure that the field is a constant (it should be a static readonly or const field)
            if (field.IsLiteral && !field.IsInitOnly && value != null)
            {
                constants.Add(field.Name, value);
            }
        }

        return constants;
    }

    public static string[] GetAsStringList(this Type type, Func<string, bool> filter)
    {
        return [.. type.GetAsStringList().Where(filter)];
    }

    /// <summary>
    /// Get all public static/readonly/const fields from a class type as a string[]
    /// </summary>
    /// <returns>string[] of the desired typeof(class)</returns>
    public static string[] GetAsStringList(this Type type)
    {
        var constants = new List<string>();

        // Get all static fields of the class
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            var value = (string?)field.GetValue(null);
            // Ensure that the field is a constant (it should be a static readonly or const field)
            if (field.IsLiteral && !field.IsInitOnly && value != null)
            {
                constants.Add(value);
            }
        }

        return [.. constants];
    }

    public static List<KeyValueEntry> GetAsKeyValuePair(this Type type)
    {
        var constants = new List<KeyValueEntry>();

        // Get all static fields of the class
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var value = (string?)field.GetValue(null);

            // Ensure that the field is a constant (it should be a static readonly or const field)
            if (field.IsLiteral && !field.IsInitOnly && value != null)
            {
                constants.Add(new KeyValueEntry() { Key = field.Name, Value = value });
            }
        }

        return constants;
    }
}