using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Commons;

public static class Constants
{
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
            }
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
            public const string Oid = "1.3.6.1.4.1.19376.1.2";
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

        public static class CodeValues
        {
            public static List<string> IheFormatCodes =
            [
                "urn:ihe:iti:xds:2017:mimeTypeSufficient",
                "urn:no:ehelse:document:pdf",
                "urn:no:ehelse:document:text",
                "urn:no:kith:xmlstds:henvisning",
                "urn:no:ehelse:document:image",
            ];

            public static List<ConceptType> ConfidentialityCodes = new()
            {
                new()
                {
                    code = "N",
                    codeSystemName = "2.16.578.1.12.4.1.1.9603",
                    displayName = "Normal"
                }
            };

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
        public static class Profile
        {
            public const string MinimalProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Minimal.ProvideBundle";
            public const string UncontainedComprehensiveProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.UnContained.Comprehensive.ProvideBundle";
            public const string ComprehensiveProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.ProvideBundle";
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
            public const char Amp = '&';
            public const char Hatt = '^';
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

        public static class CodeSystems
        {
            public const string IsoHealthRecordLifecycleEvent = "http://terminology.hl7.org/CodeSystem/iso-21089-lifecycle";
        }
    }

    public static class Oid
    {
        // The correct "system"-value for OID
        public const string System = "urn:ietf:rfc:3986";

        public const string Fnr = "2.16.578.1.12.4.1.4.1";
        public const string Dnr = "2.16.578.1.12.4.1.4.2";
        public const string Hnr = "2.16.578.1.12.4.1.4.3";
        public const string Hpr = "2.16.578.1.12.4.1.4.4";
        public const string ReshId = "2.16.578.1.12.4.1.4.102";
        public const string Brreg = "2.16.578.1.12.4.1.4.101";
        public const string Nhn = "2.16.578.1.12.4.5";

        public static class CodeSystems
        {
            public static class Volven
            {
                public const string Gender = "2.16.578.1.12.4.1.1.3101";
                public const string DocumentType = "2.16.578.1.12.4.1.1.9602";
                public static class ConfidentialityCode
                {
                    public const string Oid = "2.16.578.1.12.4.1.1.9603";

                    /// <summary> Normal</summary>
                    public const string N = "N";
                    /// <summary> Nektet, alle dokumenter</summary>
                    public const string NORN_ALL = "NORN_ALL";
                    /// <summary> Nektet, duplikat</summary>
                    public const string NORN_DUP = "NORN_DUP";
                    /// <summary> Nektet, eget ønske</summary>
                    public const string NORN_EPO = "NORN_EPO";
                    /// <summary> Nektet, fare for helsepersonell</summary>
                    public const string NORN_FFH = "NORN_FFH";
                    /// <summary> Nektet, fare for liv</summary>
                    public const string NORN_FFL = "NORN_FFL";
                    /// <summary> Nektet, foreldet</summary>
                    public const string NORN_FOR = "NORN_FOR";
                    /// <summary> Nektet, foreldreansvarlig</summary>
                    public const string NORN_FORANS = "NORN_FORANS";
                    /// <summary> Nektet, forsvarlig pasientbehandling</summary>
                    public const string NORN_FPB = "NORN_FPB";
                    /// <summary> Nektet, klart utilrådelig</summary>
                    public const string NORN_KUT = "NORN_KUT";
                    /// <summary> Nektet, ungdom</summary>
                    public const string NORN_UNGDOM = "NORN_UNGDOM";
                    /// <summary> Sperret</summary>
                    public const string NORS = "NORS";
                    /// <summary> Utsatt innsyn for innbygger</summary>
                    public const string NORU = "NORU";

                }

                public const string EventCode = "2.16.578.1.12.4.1.1.7210";


            }

            public static class Hl7
            {
                public static class ConfidentialityCode
                {
                    public const string Oid = "2.16.840.1.113883.5.25";

                    /// <summary>low</summary>
                    public const string Low = "L";
                    /// <summary>moderate</summary>
                    public const string Moderate = "M";
                    /// <summary>normal</summary>
                    public const string Normal = "N";
                    /// <summary>restricted</summary>
                    public const string Restricted = "R";
                    /// <summary>unrestricted</summary>
                    public const string Unrestricted = "U";
                    /// <summary>veryrestricted</summary>
                    public const string VeryRestricted = "V";

                }
                public static class AuditEventId
                {
                    public const string Oid = "2.16.840.1.113883.4.642.3.462";
                }

                public static class PurposeOfUse
                {
                    public const string Oid = "2.16.840.1.113883.1.11.20448";
                    /// <summary>healthcare marketing</summary>
                    public const string HMARKT = "HMARKT";
                    /// <summary>healthcare operations</summary>
                    public const string HOPERAT = "HOPERAT";
                    /// <summary>care management</summary>
                    public const string CAREMGT = "CAREMGT";
                    /// <summary>donation</summary>
                    public const string DONAT = "DONAT";
                    /// <summary>fraud</summary>
                    public const string FRAUD = "FRAUD";
                    /// <summary>government</summary>
                    public const string GOV = "GOV";
                    /// <summary>health accreditation</summary>
                    public const string HACCRED = "HACCRED";
                    /// <summary>health compliance</summary>
                    public const string HCOMPL = "HCOMPL";
                    /// <summary>decedent</summary>
                    public const string HDECD = "HDECD";
                    /// <summary>directory</summary>
                    public const string HDIRECT = "HDIRECT";
                    /// <summary>healthcare delivery management</summary>
                    public const string HDM = "HDM";
                    /// <summary>legal</summary>
                    public const string HLEGAL = "HLEGAL";
                    /// <summary>health outcome measure</summary>
                    public const string HOUTCOMS = "HOUTCOMS";
                    /// <summary>health program reporting</summary>
                    public const string HPRGRP = "HPRGRP";
                    /// <summary>health quality improvement</summary>
                    public const string HQUALIMP = "HQUALIMP";
                    /// <summary>health system administration</summary>
                    public const string HSYSADMIN = "HSYSADMIN";
                    /// <summary>labeling</summary>
                    public const string LABELING = "LABELING";
                    /// <summary>metadata management</summary>
                    public const string METAMGT = "METAMGT";
                    /// <summary>member administration</summary>
                    public const string MEMADMIN = "MEMADMIN";
                    /// <summary>military command</summary>
                    public const string MILCDM = "MILCDM";
                    /// <summary>patient administration</summary>
                    public const string PATADMIN = "PATADMIN";
                    /// <summary>patient safety</summary>
                    public const string PATSFTY = "PATSFTY";
                    /// <summary>performance measure</summary>
                    public const string PERFMSR = "PERFMSR";
                    /// <summary>records management</summary>
                    public const string RECORDMGT = "RECORDMGT";
                    /// <summary>system development</summary>
                    public const string SYSDEV = "SYSDEV";
                    /// <summary>test health data</summary>
                    public const string HTEST = "HTEST";
                    /// <summary>training</summary>
                    public const string TRAIN = "TRAIN";
                    /// <summary>healthcare payment</summary>
                    public const string HPAYMT = "HPAYMT";
                    /// <summary>claim attachment</summary>
                    public const string CLMATTCH = "CLMATTCH";
                    /// <summary>coverage authorization</summary>
                    public const string COVAUTH = "COVAUTH";
                    /// <summary>coverage under policy or program</summary>
                    public const string COVERAGE = "COVERAGE";
                    /// <summary>eligibility determination</summary>
                    public const string ELIGDTRM = "ELIGDTRM";
                    /// <summary>eligibility verification</summary>
                    public const string ELIGVER = "ELIGVER";
                    /// <summary>enrollment</summary>
                    public const string ENROLLM = "ENROLLM";
                    /// <summary>military discharge</summary>
                    public const string MILDCRG = "MILDCRG";
                    /// <summary>remittance advice</summary>
                    public const string REMITADV = "REMITADV";
                    /// <summary>healthcare research</summary>
                    public const string HRESCH = "HRESCH";
                    /// <summary>biomedical research</summary>
                    public const string BIORCH = "BIORCH";
                    /// <summary>clinical trial research</summary>
                    public const string CLINTRCH = "CLINTRCH";
                    /// <summary>clinical trial research without patient care</summary>
                    public const string CLINTRCHNPC = "CLINTRCHNPC";
                    /// <summary>clinical trial research with patient care</summary>
                    public const string CLINTRCHPC = "CLINTRCHPC";
                    /// <summary>preclinical trial research</summary>
                    public const string PRECLINTRCH = "PRECLINTRCH";
                    /// <summary>disease specific healthcare research</summary>
                    public const string DSRCH = "DSRCH";
                    /// <summary>population origins or ancestry healthcare research</summary>
                    public const string POARCH = "POARCH";
                    /// <summary>translational healthcare research</summary>
                    public const string TRANSRCH = "TRANSRCH";
                    /// <summary>patient requested</summary>
                    public const string PATRQT = "PATRQT";
                    /// <summary>family requested</summary>
                    public const string FAMRQT = "FAMRQT";
                    /// <summary>power of attorney</summary>
                    public const string PWATRNY = "PWATRNY";
                    /// <summary>support network</summary>
                    public const string SUPNWK = "SUPNWK";
                    /// <summary>public health</summary>
                    public const string PUBHLTH = "PUBHLTH";
                    /// <summary>disaster</summary>
                    public const string DISASTER = "DISASTER";
                    /// <summary>threat</summary>
                    public const string THREAT = "THREAT";
                    /// <summary>treatment</summary>
                    public const string TREAT = "TREAT";
                    /// <summary>clinical trial</summary>
                    public const string CLINTRL = "CLINTRL";
                    /// <summary>coordination of care</summary>
                    public const string COC = "COC";
                    /// <summary>Emergency Treatment</summary>
                    public const string ETREAT = "ETREAT";
                    /// <summary>break the glass</summary>
                    public const string BTG = "BTG";
                    /// <summary>emergency room treatment</summary>
                    public const string ERTREAT = "ERTREAT";
                    /// <summary>population health</summary>
                    public const string POPHLTH = "POPHLTH";
                }
            }

            public static class OtherIsoDerived
            {
                public static class PurposeOfUse
                {
                    public const string Oid = "1.0.14265.1";
                    public const string ClinicalCare_1 = "1";
                    public const string EmergencyCare_2 = "2";
                    public const string Management_5 = "5";
                    public const string SubjectOfCare_13 = "13";
                }
            }
        }

        public static class Saml
        {
            public static class Acp
            {
                // Citizen OID values

                /// <summary>
                /// CUSTOM OID: No representation overrides (represents themself)
                /// </summary>
                public const string NullValue = "urn:oid:2.16.578.1.12.4.1.7.2.1.0";

                /// <summary>
                /// Represent citizen under 12 years of age
                /// </summary>
                public const string RepresentCitizenUnder12 = "urn:oid:2.16.578.1.12.4.1.7.2.1.1";

                /// <summary>
                /// Represent another cititzen (Power of Attorney)
                /// </summary>
                public const string RepresentAnotherCitizen = "urn:oid:2.16.578.1.12.4.1.7.2.1.2";

                /// <summary>
                /// Represent citizen unable to give consent
                /// </summary>
                public const string RepresentedUnableToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.3";

                // Healthcare practitioner OID values

                /// <summary>
                /// Healthcare professional [subject] is not obliged to retrieve patient's consent to [resource] open and see patient's healthcare data, e.g. "patient's regular physician" (fastlege)
                /// </summary>
                public const string NotObligedToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.4";

                /// <summary>
                /// Healthcare professional [subject] has been given explicit consent from patient [resource] to open and see patient's healthcare data, including locked data
                /// </summary>
                public const string ExcplicitConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.5";

                /// <summary>
                /// Healthcare professional [subject] is not able to retrieve consent from current patient [resource] (e.g. patient is unconscious)
                /// </summary>
                public const string UnableToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.6";

                /// <summary>
                /// Healthcare professional [subject] has documented reasons to unlock all available healthcare data for current patient [resource] in an emergency/catastrophic situation
                /// </summary>
                public const string ExceptionToConcent = "urn:oid:2.16.578.1.12.4.1.7.2.1.7";

                /// <summary>
                /// Healthcare professional [subject] has retrieved consent from patient [resource] to open and see patient's healthcare data
                /// </summary>
                public const string HasConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.8";

            }

            public static class Bppc
            {
                /// <summary>
                /// CUSTOM OID: Null value
                /// </summary>
                public const string NullValue = "urn:oid:2.16.578.1.12.4.1.7.2.2.0";

                /// <summary>
                /// Consent from an analog channel
                /// </summary>
                public const string AnalogChannel = "urn:oid:2.16.578.1.12.4.1.7.2.2.1";

                /// <summary>
                /// Consent from a digital channel
                /// </summary>
                public const string DigitalChannel = "urn:oid:2.16.578.1.12.4.1.7.2.2.2";
            }

        }
    }
    public static class MimeTypes
    {
        public const string FhirJson = "application/fhir+json";
        public const string Hl7v3Xml = "application/hl7-v3+xml";
        public const string Json = "application/json";
        public const string SoapXml = "application/soap+xml";
        public const string MultipartRelated = "multipart/related";
        public const string XopXml = "application/xop+xml";
        public const string SevenZip = "application/x-7z-compressed";
        public const string Acc = "audio/aac";
        public const string Avi = "video/x-msvideo";
        public const string Doc = "application/msword";
        public const string Docm = "application/vnd.ms-word.document.macroEnabled.12";
        public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string Gif = "image/gif";
        public const string Html = "text/html";
        public const string Jpeg = "image/jpeg";
        public const string Mp4 = "video/mp4";
        public const string Mpeg = "video/mpeg";
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
        public const string Text = "text/plain";
        public const string Tiff = "image/tiff";
        public const string Binary = "application/octet-stream";
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
            public const string Unknown = "Unknown";
        }

        public static class Functions
        {
            public const string StringEqual = "urn:oasis:names:tc:xacml:1.0:function:string-equal";
            public const string StringIsIn = "urn:oasis:names:tc:xacml:1.0:function:string-is-in";
            public const string StringAtLeastOneMemberOf = "urn:oasis:names:tc:xacml:1.0:function:string-at-least-one-member-of";
            public const string StringBag = "urn:oasis:names:tc:xacml:1.0:function:string-bag";
            public const string And = "urn:oasis:names:tc:xacml:1.0:function:and";
            public const string Or = "urn:oasis:names:tc:xacml:1.0:function:or";
            public const string StringOneAndOnly = "urn:oasis:names:tc:xacml:1.0:function:string-one-and-only";
            public const string Not = "urn:oasis:names:tc:xacml:1.0:function:not";
        }
        public static class CombiningAlgorithms
        {
            // XACML 1.0 / 1.1 Rule Combining Algorithms
            public const string V20_RuleCombining_DenyOverrides = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:deny-overrides";
            public const string V20_RuleCombining_PermitOverrides = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:permit-overrides";
            public const string V20_RuleCombining_FirstApplicable = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:first-applicable";
            public const string V20_RuleCombining_OrderedDenyOverrides = "urn:oasis:names:tc:xacml:1.1:rule-combining-algorithm:ordered-denyoverrides";
            public const string V20_RuleCombining_OrderedPermitOverrides = "urn:oasis:names:tc:xacml:1.1:rule-combining-algorithm:ordered-permitoverrides";

            // XACML 1.0 / 1.1 Policy Combining Algorithms
            public const string V20_PolicyCombining_DenyOverrides = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides";
            public const string V20_PolicyCombining_PermitOverrides = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:permit-overrides";
            public const string V20_PolicyCombining_FirstApplicable = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:first-applicable";
            public const string V20_PolicyCombining_OnlyOneApplicable = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:only-one-applicable";
            public const string V20_PolicyCombining_OrderedDenyOverrides = "urn:oasis:names:tc:xacml:1.1:policy-combining-algorithm:ordered-denyoverrides";
            public const string V20_PolicyCombining_OrderedPermitOverrides = "urn:oasis:names:tc:xacml:1.1:policy-combining-algorithm:ordered-permitoverrides";

            // XACML 3.0 Rule Combining Algorithms
            public const string V30_RuleCombining_DenyOverrides = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:deny-overrides";
            public const string V30_RuleCombining_PermitOverrides = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:permit-overrides";
            public const string V30_RuleCombining_FirstApplicable = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:first-applicable";
            public const string V30_RuleCombining_DenyUnlessPermit = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:deny-unless-permit";
            public const string V30_RuleCombining_PermitUnlessDeny = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:permit-unless-deny";

            // XACML 3.0 Policy Combining Algorithms
            public const string V30_PolicyCombining_DenyOverrides = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:deny-overrides";
            public const string V30_PolicyCombining_PermitOverrides = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:permit-overrides";
            public const string V30_PolicyCombining_FirstApplicable = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:first-applicable";
            public const string V30_PolicyCombining_DenyUnlessPermit = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:deny-unless-permit";
            public const string V30_PolicyCombining_PermitUnlessDeny = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:permit-unless-deny";
        }

        public static class Attribute
        {
            public const string ActionId = "urn:oasis:names:tc:xacml:1.0:action:action-id";
            public const string SubjectId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
            public const string ResourceId = "urn:oasis:names:tc:xacml:2.0:resource:resource-id";
            public const string Role = "urn:oasis:names:tc:xspa:1.0:subject:role";
        }

        public static class CustomAttributes
        {
            public const string BaseUrn = "urn:no:nhn:xcads:";
            public const string DocumentEntryPatientIdentifier = BaseUrn + "document:patient-identifier";
            public const string AdhocQueryPatientIdentifier = BaseUrn + "adhocquery:patient-identifier";
            public const string DocumentUniqueId = BaseUrn + "document:uniqueid";
            public const string RepositoryUniqueId = BaseUrn + "document:repositoryuniqueid";
            public const string HomeCommunityId = BaseUrn + "document:homecommunityid";
            public const string SamlNameId = BaseUrn + "saml:nameid";
            public const string AppliesTo = BaseUrn + "xacml:appliesto";
            public const string UnknownAttribute = BaseUrn + "xacml:unknownattribute:";
        }

        public static class Category
        {
            public const string V30_Subject = "urn:oasis:names:tc:xacml:3.0:attribute-category:access-subject";
            public const string V30_Resource = "urn:oasis:names:tc:xacml:3.0:attribute-category:resource";
            public const string V30_Action = "urn:oasis:names:tc:xacml:3.0:attribute-category:action";
            public const string V30_Environment = "urn:oasis:names:tc:xacml:3.0:attribute-category:environment";

            public const string V20_Subject = "urn:oasis:names:tc:xacml:2.0:attribute-category:access-subject";
            public const string V20_Resource = "urn:oasis:names:tc:xacml:2.0:attribute-category:resource";
            public const string V20_Action = "urn:oasis:names:tc:xacml:2.0:attribute-category:action";
            public const string V20_Environment = "urn:oasis:names:tc:xacml:2.0:attribute-category:environment";
        }

        public static class DataType
        {
            public const string String = "http://www.w3.org/2001/XMLSchema#string";
            public const string Name = "urn:oasis:names:tc:xacml:1.0:data-type:rfc822Name";
            public const string Uri = "http://www.w3.org/2001/XMLSchema#anyURI";
            public const string XPath = "urn:oasis:names:tc:xacml:3.0:data-type:xpathExpression";
            public const string Date = "http://www.w3.org/2001/XMLSchema#date";
            public const string DateTime = "http://www.w3.org/2001/XMLSchema#dateTime";
        }

        public static class Namespace
        {
            public const string WD17 = "urn:oasis:names:tc:xacml:3.0:core:schema:wd-17";
            public const string Policy_OS = "urn:oasis:names:tc:xacml:2.0:policy:schema:os";
            public const string Context_OS = "urn:oasis:names:tc:xacml:2.0:context:schema:os";
        }

        public static class StatusCodes
        {
            public const string MissingAttribute = "urn:oasis:names:tc:xacml:1.0:status:missing-attribute";
            public const string Ok = "urn:oasis:names:tc:xacml:1.0:status:ok";
            public const string ProcessingError = "urn:oasis:names:tc:xacml:1.0:status:processing-error";
            public const string SyntaxError = "urn:oasis:names:tc:xacml:1.0:status:syntax-error ";
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
        public const string XdsPolicy = "XdsPolicy";
        public const string XdsPolicyWithDPoP = "XdsPolicyWithDPoP";
        public const string RequiredClaimsPolicy = "RequiredClaimsPolicy";
        public const string DPoPTokenAuthenticationScheme = "dpop_token_authentication_scheme";
        public const string BearerTokenAuthenticationScheme = "bearer_token_authentication_scheme";

        public const string ClientIdClaimType = "client_id";
        public const string AuthTime = "auth_time";
        public const string PidClaimType = "helseid://claims/identity/pid";
        public const string SecurityLevelClaimType = "helseid://claims/identity/security_level";
        public const string HprNumberClaimType = "helseid://claims/hpr/hpr_number";
        public const string TillitsrammeverkClaimType = "nhn:tillitsrammeverk:parameters";
        public const string Scope = "scope";
        public const string FastlegeClaimType = "fastlege";

    }

    public static class AuditLogging
    {
        public class XcaAction
        {
            public const string ITI18 = "ITI-18";
            public const string ITI39 = "ITI-39";
        }

        // purposeOfUse Code values
        public const string TREAT = "TREAT";
        public const string ETREAT = "ETREAT";
        public const string COC = "COC";

        // old purposeOfUse Codes
        public const string subject_of_care = "1"; // TREAT
        public const string emergency_care = "2"; // ETREAT
        public const string management_qa = "5"; // COC

        // citizen codes
        public const string OPPSLAG_HELSENORGE = "13";

        // ACP-fields
        public class ACP
        {
            public const string segselv = "segselv";
            public const string fullmakt = "2.16.578.1.12.4.1.7.2.1.4";
            public const string cannot_consent = "2.16.578.1.12.4.1.7.2.1.3";
            public const string verge = "2.16.578.1.12.4.1.7.2.1.2";
            public const string foreldre = "2.16.578.1.12.4.1.7.2.1.1";
        }

        public class LoggerNames
        {
            public const string AT_SENSE_XUA = "at.sense.xua.module.handler.XUAOutHandler";
            //public const string HTTP_WIRE = "httpclient.wire.content";   
            public const string HTTP_WIRE = "org.apache.http.wire";
            public const string ACTION_CLASS = "at.sense.util.operationtemplate.OperationLogger";
            public const string AUDIT_MESSAGE_WRITER = "at.sense.logging.atna.content.AuditMessageWriter";
        }

        public class RegexPatterns
        {
            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string ConfidentialityCode = @"confidentialityCode\s+code=\\?\""([^\""]+)\\\""";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string DocumentSourceName = @"<name>([^<]+)</name>";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string DocumentTitle = @"<title>([^<]+)</title>";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string DocumentEffectiveTime = @"<effectiveTime value=\\?\""([^\""]+)\\?\""";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string HomeCommunityId = @"<ns.:HomeCommunityId>([^<]+)";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string RepositoryUniqueId = @"<ns.:RepositoryUniqueId>([^<]+)";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string DocumentUniqueId = @"<ns.:DocumentUniqueId>([^<]+)";

            [StringSyntax(StringSyntaxAttribute.Regex)]
            public const string DocumentIdWithOid = @"[\.\d]+\^[\w\d]+";
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
                constants = [.. constants, new KeyValueEntry() { Key = field.Name, Value = value }];
            }
        }

        return constants;
    }
}
