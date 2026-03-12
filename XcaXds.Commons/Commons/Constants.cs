using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Models.Custom;

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
            public const string System = "1.3.6.1.4.1.19376.1.2";

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
            public static string[] IheFormatCodes =
            [
                "urn:ihe:iti:xds:2017:mimeTypeSufficient",
                "urn:no:ehelse:document:pdf",
                "urn:no:ehelse:document:text",
                "urn:no:kith:xmlstds:henvisning",
                "urn:no:ehelse:document:image",
            ];
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
    }

    public static class CodeSystems
    {
        public static class Volven
        {
            public static class Gender_3101
            {
                public const string System = "2.16.578.1.12.4.1.1.3101";

                ///<summary>Ikke kjent</summary>
                public const string Unknown = "0";
                ///<summary>Mann</summary>
                public const string Male = "1";
                ///<summary>Kvinne</summary>
                public const string Female = "2";
                ///<summary>Ikke spesifisert</summary>
                public const string Unspecified = "9";
            }

            public static class EventCode_7010
            {
                public const string System = "2.16.578.1.12.4.1.1.7010";
            }

            public static class EventCode_7210
            {
                public const string System = "2.16.578.1.12.4.1.1.7210";
            }

            public static class EventCode_7220
            {
                public const string System = "2.16.578.1.12.4.1.1.7220";
            }

            public static class EventCode_7270
            {
                public const string System = "2.16.578.1.12.4.1.1.7270";
            }

            public static class FacilityType_1303
            {
                public const string System = "2.16.578.1.12.4.1.1.1303";

                /// <summary>Alminnelige somatiske sykehus</summary>
                public const string _86_101 = "86.101";
                /// <summary>Somatiske spesialsykehus</summary>
                public const string _86_102 = "86.102";
                /// <summary>Andre somatiske spesialinstitusjoner</summary>
                public const string _86_103 = "86.103";
                /// <summary>Institusjoner i psykisk helsevern for voksne</summary>
                public const string _86_104 = "86.104";
                /// <summary>Institusjoner i psykisk helsevern for barn og unge</summary>
                public const string _86_105 = "86.105";
                /// <summary>Rusmiddelinstitusjoner</summary>
                public const string _86_106 = "86.106";
                /// <summary>Rehabiliterings- og opptreningsinstitusjoner</summary>
                public const string _86_107 = "86.107";
                /// <summary>Allmenn legetjeneste</summary>
                public const string _86_211 = "86.211";
                /// <summary>Somatiske poliklinikker</summary>
                public const string _86_212 = "86.212";
                /// <summary>Spesialisert legetjeneste, unntatt psykiatrisk legetjeneste</summary>
                public const string _86_221 = "86.221";
                /// <summary>Legetjenester innen psykisk helsevern</summary>
                public const string _86_222 = "86.222";
                /// <summary>Poliklinikker i psykisk helsevern for voksne</summary>
                public const string _86_223 = "86.223";
                /// <summary>Poliklinikker i psykisk helsevern for barn og unge</summary>
                public const string _86_224 = "86.224";
                /// <summary>Rusmiddelpoliklinikker</summary>
                public const string _86_225 = "86.225";
                /// <summary>Tannhelsetjenester</summary>
                public const string _86_230 = "86.230";
                /// <summary>Hjemmesykepleie</summary>
                public const string _86_901 = "86.901";
                /// <summary>Fysioterapitjeneste</summary>
                public const string _86_902 = "86.902";
                /// <summary>Helsestasjons- og skolehelsetjeneste</summary>
                public const string _86_903 = "86.903";
                /// <summary>Annen forebyggende helsetjeneste</summary>
                public const string _86_904 = "86.904";
                /// <summary>Klinisk psykologtjeneste</summary>
                public const string _86_905 = "86.905";
                /// <summary>Medisinske laboratorietjenester</summary>
                public const string _86_906 = "86.906";
                /// <summary>Ambulansetjenester</summary>
                public const string _86_907 = "86.907";
                /// <summary>Andre helsetjenester</summary>
                public const string _86_909 = "86.909";
                /// <summary>Somatiske spesialsykehjem</summary>
                public const string _87_101 = "87.101";
                /// <summary>Somatiske sykehjem</summary>
                public const string _87_102 = "87.102";
                /// <summary>Psykiatriske sykehjem</summary>
                public const string _87_201 = "87.201";
                /// <summary>Omsorgsinstitusjoner for rusmiddelmisbrukere</summary>
                public const string _87_202 = "87.202";
                /// <summary>Bofellesskap for psykisk utviklingshemmede</summary>
                public const string _87_203 = "87.203";
                /// <summary>Aldershjem</summary>
                public const string _87_301 = "87.301";
                /// <summary>Bofellesskap for eldre og funksjonshemmede med fast tilknyttet personell hele døgnet</summary>
                public const string _87_302 = "87.302";
                /// <summary>Bofellesskap for eldre og funksjonshemmede med fast tilknyttet personell deler av døgnet</summary>
                public const string _87_303 = "87.303";
                /// <summary>Avlastningsboliger/-institusjoner</summary>
                public const string _87_304 = "87.304";
                /// <summary>Barneboliger</summary>
                public const string _87_305 = "87.305";
                /// <summary>Institusjoner innen barne- og ungdomsvern</summary>
                public const string _87_901 = "87.901";
                /// <summary>Omsorgsinstitusjoner ellers</summary>
                public const string _87_909 = "87.909";
                /// <summary>Hjemmehjelp</summary>
                public const string _88_101 = "88.101";
                /// <summary>Dagsentra/aktivitetssentra for eldre og funksjonshemmede</summary>
                public const string _88_102 = "88.102";
                /// <summary>Eldresentre</summary>
                public const string _88_103 = "88.103";
                /// <summary>Barnehager</summary>
                public const string _88_911 = "88.911";
                /// <summary>Barneparker og dagmammaer</summary>
                public const string _88_912 = "88.912";
                /// <summary>Skolefritidsordninger</summary>
                public const string _88_913 = "88.913";
                /// <summary>Fritidsklubber for barn og ungdom</summary>
                public const string _88_914 = "88.914";
                /// <summary>Barneverntjenester</summary>
                public const string _88_991 = "88.991";
                /// <summary>Familieverntjenester</summary>
                public const string _88_992 = "88.992";
                /// <summary>Arbeidstrening for ordinært arbeidsmarked</summary>
                public const string _88_993 = "88.993";
                /// <summary>Varig tilrettelagt arbeid</summary>
                public const string _88_994 = "88.994";
                /// <summary>Sosiale velferdsorganisasjoner</summary>
                public const string _88_995 = "88.995";
                /// <summary>Asylmottak</summary>
                public const string _88_996 = "88.996";
                /// <summary>Sosialtjenester for rusmiddelmisbrukere uten botilbud</summary>
                public const string _88_997 = "88.997";
                /// <summary>Kommunale sosialkontortjenester</summary>
                public const string _88_998 = "88.998";
                /// <summary>Andre sosialtjenester uten botilbud</summary>
                public const string _88_999 = "88.999";
            }

            public static class PracticeSetting_8651
            {
                public const string System = "2.16.578.1.12.4.1.1.8651";

                ///<summary>Operasjon</summary>
                public const string A01 = "A01";
                ///<summary>Observasjonsenhet</summary>
                public const string A02 = "A02";
                ///<summary>Intensivenhet</summary>
                public const string A03 = "A03";
                ///<summary>Overvåkningsenhet</summary>
                public const string A04 = "A04";
                ///<summary>Intermediærenhet</summary>
                public const string A05 = "A05";
            }

            public static class PracticeSetting_8653
            {
                public const string System = "2.16.578.1.12.4.1.1.8653";

                /// <summary>Pleietjenester</summary>
                public const string _1 = "1";
                /// <summary>Pasienthotell</summary>
                public const string _2 = "2";
                /// <summary>Pasientmottak, elektiv</summary>
                public const string _3 = "3";
                /// <summary>Legevakt</summary>
                public const string _4 = "4";
                /// <summary>Vurdering av henvisning</summary>
                public const string _5 = "5";
                /// <summary>Akuttmottak	</summary>
                public const string _6 = "6";
                /// <summary>Ambulansetjeneste, ordinær</summary>
                public const string _7 = "7";
                /// <summary>Luftambulanse</summary>
                public const string _8 = "8";
                /// <summary>AMK-sentral</summary>
                public const string _9 = "9";
            }

            public static class PracticeSetting_8654
            {
                public const string System = "2.16.578.1.12.4.1.1.8654";

                /// <summary>Bildediagnostikk</summary>
                public const string B = "B";
                /// <summary>Røntgen</summary>
                public const string B01 = "B01";
                /// <summary>Ultralyd</summary>
                public const string B02 = "B02";
                /// <summary>Angiografi</summary>
                public const string B03 = "B03";
                /// <summary>Tomografi MR</summary>
                public const string B04 = "B04";
                /// <summary>Tomografi CT</summary>
                public const string B05 = "B05";
                /// <summary>Nukleærmedisin</summary>
                public const string B06 = "B06";
                /// <summary>Nevroradiologi</summary>
                public const string B07 = "B07";
                /// <summary>Intervensjonsradiologi</summary>
                public const string B08 = "B08";
                /// <summary>Laboratoriefag</summary>
                public const string L = "L";
                /// <summary>Klinisk farmakologi</summary>
                public const string L01 = "L01";
                /// <summary>Immunologi, allergologi og transfusjonsmedisin</summary>
                public const string L02 = "L02";
                /// <summary>Immunologi og allergologi</summary>
                public const string L0201 = "L0201";
                /// <summary>Transfusjonsmedisin</summary>
                public const string L0202 = "L0202";
                /// <summary>Medisinsk biokjemi</summary>
                public const string L03 = "L03";
                /// <summary>Medisinsk mikrobiologi</summary>
                public const string L04 = "L04";
                /// <summary>Patologi</summary>
                public const string L06 = "L06";
                /// <summary>Klinisk nevrofysiologi</summary>
                public const string L07 = "L07";
                /// <summary>Nevrovaskulært laboratorium</summary>
                public const string L08 = "L08";
                /// <summary>Nevroimmunologisk laboratorium</summary>
                public const string L09 = "L09";
                /// <summary>Cytogenetikk og molekylærgenetikk</summary>
                public const string L10 = "L10";

            }

            public static class PracticeSetting_8655
            {
                public const string System = "2.16.578.1.12.4.1.1.8655";

                /// <summmary>Andre helsehjelpsområder</summary>
                public const string A = "A";
                /// <summmary>Sosionomtjenester</summary>
                public const string A01 = "A01";
                /// <summmary>Ergoterapi</summary>
                public const string A02 = "A02";
                /// <summmary>Fysioterapi</summary>
                public const string A03 = "A03";
                /// <summmary>Kiropraktikk</summary>
                public const string A04 = "A04";
                /// <summmary>Ernæringsfysiologi</summary>
                public const string A05 = "A05";
                /// <summmary>Tannhelse</summary>
                public const string A06 = "A06";
                /// <summmary>Audiografi</summary>
                public const string A07 = "A07";
                /// <summmary>Spesialpedagogikk</summary>
                public const string A08 = "A08";
                /// <summmary>Logopedi</summary>
                public const string A09 = "A09";
                /// <summmary>Farmasi</summary>
                public const string A10 = "A10";
                /// <summmary>Yrkes- og arbeidsmedisin</summary>
                public const string A11 = "A11";
                /// <summmary>Psykologtjeneste</summary>
                public const string A12 = "A12";
                /// <summmary>Helsehjelp knyttet til habilitering og rehabilitering</summary>
                public const string H = "H";
                /// <summmary>Barnehabilitering</summary>
                public const string H07 = "H07";
                /// <summmary>Voksenhabilitering</summary>
                public const string H08 = "H08";
                /// <summmary>Rehabilitering</summary>
                public const string H09 = "H09";
                /// <summmary>Psykisk helsevern</summary>
                public const string P = "P";
                /// <summmary>Psykisk helsevern for barn og unge (BUP)</summary>
                public const string PB = "PB";
                /// <summmary>Familieterapi</summary>
                public const string PB01 = "PB01";
                /// <summmary>Spiseforstyrrelser hos barn</summary>
                public const string PB02 = "PB02";
                /// <summmary>Psykisk helsevern for voksne</summary>
                public const string PV = "PV";
                /// <summmary>Spiseforstyrrelser hos voksne</summary>
                public const string PV01 = "PV01";
                /// <summmary>Psykiatrisk helsehjelp til døve</summary>
                public const string PV02 = "PV02";
                /// <summmary>Unge schizofrene</summary>
                public const string PV03 = "PV03";
                /// <summmary>Alderspsykiatrisk behandling</summary>
                public const string PV04 = "PV04";
                /// <summmary>Psykiatrisk helsehjelp til asylsøkere og flyktninger</summary>
                public const string PV05 = "PV05";
                /// <summmary>Tidlig intervensjon</summary>
                public const string PV06 = "PV06";
                /// <summmary>Pasienter med langvarig funksjonssvikt</summary>
                public const string PV07 = "PV07";
                /// <summmary>Førstegangspsykose</summary>
                public const string PV08 = "PV08";
                /// <summmary>Habilitering/Rehabilitering (psykisk helsevern for voksne)</summary>
                public const string PV09 = "PV09";
                /// <summmary>Familieterapi/behandling</summary>
                public const string PV10 = "PV10";
                /// <summmary>Sikkerhetspsykiatri</summary>
                public const string PV11 = "PV11";
                /// <summmary>Helsehjelp knyttet til rusmiddelavhengighet og annen avhengighet</summary>
                public const string R = "R";
                /// <summmary>Spilleavhengighet og annen avhengighet</summary>
                public const string R01 = "R01";
                /// <summmary>Rusmiddelavhengighet med alvorlig psykiatrisk sykdom (dobbeldiagnose)</summary>
                public const string R02 = "R02";
                /// <summmary>Rusmiddelavhengighet med langvarig funksjonssvikt</summary>
                public const string R03 = "R03";
                /// <summmary>Førstegangspsykose knyttet til rusmiddelavhengighet</summary>
                public const string R04 = "R04";
                /// <summmary>Utredning av rusmiddelavhengighet eller annen avhengighet</summary>
                public const string R05 = "R05";
                /// <summmary>Avrusning/ avgiftning/ stabilisering</summary>
                public const string R06 = "R06";
                /// <summmary>Familieterapi, parterapi og pårørendeterapi</summary>
                public const string R07 = "R07";
                /// <summmary>Legemiddelassistert rehabilitering (LAR)</summary>
                public const string R08 = "R08";
                /// <summmary>Terapeutisk samfunn, kollektiv osv.</summary>
                public const string R09 = "R09";
                /// <summmary>Innsatte under paragraf 12-soning</summary>
                public const string R10 = "R10";
                /// <summmary>Tverrfaglig spesialisert behandling av rusmiddelmisbruk</summary>
                public const string R11 = "R11";
                /// <summmary>Helsehjelp knyttet til somatisk sykdom</summary>
                public const string S = "S";
                /// <summmary>Allmennmedisin</summary>
                public const string S01 = "S01";
                /// <summmary>Kirurgi</summary>
                public const string S02 = "S02";
                /// <summmary>Generell kirurgi</summary>
                public const string S0201 = "S0201";
                /// <summmary>Barnekirurgi</summary>
                public const string S0202 = "S0202";
                /// <summmary>Bryst og endokrin kirurgi</summary>
                public const string S0203 = "S0203";
                /// <summmary>Gastroenterologisk kirurgi</summary>
                public const string S0204 = "S0204";
                /// <summmary>Karkirurgi</summary>
                public const string S0205 = "S0205";
                /// <summmary>Kjeve- og ansiktskirurgi</summary>
                public const string S0206 = "S0206";
                /// <summmary>Nevrokirurgi</summary>
                public const string S0207 = "S0207";
                /// <summmary>Ortopedisk kirurgi</summary>
                public const string S0208 = "S0208";
                /// <summmary>Plastikkirurgi</summary>
                public const string S0209 = "S0209";
                /// <summmary>Thoraxkirurgi</summary>
                public const string S0210 = "S0210";
                /// <summmary>Urologi</summary>
                public const string S0211 = "S0211";
                /// <summmary>Indremedisin</summary>
                public const string S03 = "S03";
                /// <summmary>Endokrinologi</summary>
                public const string S0301 = "S0301";
                /// <summmary>Fordøyelsessykdommer</summary>
                public const string S0302 = "S0302";
                /// <summmary>Geriatri</summary>
                public const string S0303 = "S0303";
                /// <summmary>Blodsykdommer</summary>
                public const string S0304 = "S0304";
                /// <summmary>Infeksjonsmedisin</summary>
                public const string S0305 = "S0305";
                /// <summmary>Hjertesykdommer</summary>
                public const string S0306 = "S0306";
                /// <summmary>Hjerterytmeforstyrrelser</summary>
                public const string S030601 = "S030601";
                /// <summmary>Ekkokardiografi og bildediagnostikk</summary>
                public const string S030602 = "S030602";
                /// <summmary>Klinisk kardiologi</summary>
                public const string S030603 = "S030603";
                /// <summmary>Forebyggende kardiologi</summary>
                public const string S030604 = "S030604";
                /// <summmary>Invasiv kardiologi</summary>
                public const string S030605 = "S030605";
                /// <summmary>Lungesykdommer</summary>
                public const string S0307 = "S0307";
                /// <summmary>Nyresykdommer</summary>
                public const string S0308 = "S0308";
                /// <summmary>Dialyse</summary>
                public const string S0309 = "S0309";
                /// <summmary>Fødselshjelp og kvinnesykdommer</summary>
                public const string S04 = "S04";
                /// <summmary>Generell gynekologi</summary>
                public const string S0401 = "S0401";
                /// <summmary>Gynekologisk onkologi</summary>
                public const string S0402 = "S0402";
                /// <summmary>Obstetrikk</summary>
                public const string S0403 = "S0403";
                /// <summmary>Assistert befruktning</summary>
                public const string S0404 = "S0404";
                /// <summmary>Fostermedisin</summary>
                public const string S0405 = "S0405";
                /// <summmary>Hud- og veneriske sykdommer</summary>
                public const string S05 = "S05";
                /// <summmary>Hudsykdommer</summary>
                public const string S0501 = "S0501";
                /// <summmary>Veneriske sykdommer</summary>
                public const string S0502 = "S0502";
                /// <summmary>Barnesykdommer</summary>
                public const string S06 = "S06";
                /// <summmary>Nyfødtmedisin</summary>
                public const string S0601 = "S0601";
                /// <summmary>Intensivbehandling av barn</summary>
                public const string S0602 = "S0602";
                /// <summmary>Nevrologi</summary>
                public const string S07 = "S07";
                /// <summmary>Generell nevrologi</summary>
                public const string S0701 = "S0701";
                /// <summmary>Cerebrovaskulære sykdommer</summary>
                public const string S0702 = "S0702";
                /// <summmary>Epilepsi</summary>
                public const string S0703 = "S0703";
                /// <summmary>Nevrofysiologi</summary>
                public const string S0704 = "S0704";
                /// <summmary>Anestesiologi/smertebehandling</summary>
                public const string S08 = "S08";
                /// <summmary>Øre-nese-halssykdommer</summary>
                public const string S09 = "S09";
                /// <summmary>Audiologi</summary>
                public const string S0901 = "S0901";
                /// <summmary>Laryngologi/Foniatri</summary>
                public const string S0902 = "S0902";
                /// <summmary>Balansemedisin</summary>
                public const string S0903 = "S0903";
                /// <summmary>Søvnrelaterte sykdommer</summary>
                public const string S0904 = "S0904";
                /// <summmary>Nese- og bihulesykdommer</summary>
                public const string S0905 = "S0905";
                /// <summmary>Otologi</summary>
                public const string S0906 = "S0906";
                /// <summmary>Hode- og halskirurgi</summary>
                public const string S0907 = "S0907";
                /// <summmary>Allergologi</summary>
                public const string S0908 = "S0908";
                /// <summmary>Pediatriske øre-nese-halssykdommer</summary>
                public const string S0909 = "S0909";
                /// <summmary>Øyesykdommer</summary>
                public const string S10 = "S10";
                /// <summmary>Onkologi</summary>
                public const string S11 = "S11";
                /// <summmary>Sarkomer</summary>
                public const string S1101 = "S1101";
                /// <summmary>Revmatologi</summary>
                public const string S12 = "S12";
                /// <summmary>Tverrfaglig ryggbehandling</summary>
                public const string S13 = "S13";
                /// <summmary>Palliativ medisin</summary>
                public const string S14 = "S14";
                /// <summmary>Medisinsk genetikk</summary>
                public const string S15 = "S15";
                /// <summmary>Fysikalsk medisin og rehabilitering</summary>
                public const string S16 = "S16";
            }

            public static class PracticeSetting_8663
            {
                public const string System = "2.16.578.1.12.4.1.1.8663";

                /// <summary>Legevakt</summary>
                public const string KA02 = "KA02";
                /// <summary>Kommuneoverlege</summary>
                public const string KA03 = "KA03";
                /// <summary>Smittevern</summary>
                public const string KA0301 = "KA0301";
                /// <summary>Migrasjonshelse</summary>
                public const string KA04 = "KA04";
                /// <summary>Kommunal nettlege</summary>
                public const string KA05 = "KA05";
                /// <summary>Sosialtjeneste</summary>
                public const string KD01 = "KD01";
                /// <summary>Saksbehandling</summary>
                public const string KD0501 = "KD0501";
                /// <summary>Helsestasjons- og skolehelsetjeneste</summary>
                public const string KF01 = "KF01";
                /// <summary>Helsestasjon for ungdom</summary>
                public const string KF0103 = "KF0103";
                /// <summary>Legetjeneste ved sykehjem mv.</summary>
                public const string KP01 = "KP01";
                /// <summary>Sykepleietjeneste</summary>
                public const string KP02 = "KP02";
                /// <summary>Fengselshelsetjeneste</summary>
                public const string KX01 = "KX01";
                /// <summary>Frisklivssentral</summary>
                public const string KX04 = "KX04";
                /// <summary>Øyeblikkelig hjelp døgntilbud (ØHD)</summary>
                public const string KX05 = "KX05";
                /// <summary>Kreftkoordinator</summary>
                public const string KX06 = "KX06";
                /// <summary>Demenskoordinator</summary>
                public const string KX07 = "KX07";
                /// <summary>Familieteam</summary>
                public const string KX12 = "KX12";
                /// <summary>Barnevern</summary>
                public const string KX15 = "KX15";
                /// <summary>Pedagogisk-psykologisk tjeneste (PPT)</summary>
                public const string KX16 = "KX16";
                /// <summary>Barnevernvakt</summary>
                public const string KX18 = "KX18";
            }

            public static class TypeCode_9602
            {
                public const string System = "2.16.578.1.12.4.1.1.9602";

                /// <summary>Kriseplan</summary>
                public const string A01_2 = "A01-2";
                /// <summary>Individuell plan</summary>
                public const string A02_2 = "A02-2";
                /// <summary>Epikrise</summary>
                public const string A03_2 = "A03-2";
                /// <summary>Sykepleiesammenfatning</summary>
                public const string A04_2 = "A04-2";
                /// <summary>Fysioterapisammenfatning</summary>
                public const string A05_2 = "A05-2";
                /// <summary>Ergoterapisammenfatning</summary>
                public const string A06_2 = "A06-2";
                /// <summary>Psykologsammenfatning</summary>
                public const string A07_2 = "A07-2";
                /// <summary>Sosionomsammenfatning</summary>
                public const string A08_2 = "A08-2";
                /// <summary>Ernæringsfysiologsammenfatning</summary>
                public const string A09_2 = "A09-2";
                /// <summary>Annet fagpersonell sammenfatning</summary>
                public const string A10_2 = "A10-2";
                /// <summary>Tverrfaglig sammenfatning</summary>
                public const string A11_2 = "A11-2";
                /// <summary>Utskrivings-/Pasientorientering</summary>
                public const string A12_2 = "A12-2";
                /// <summary>Poliklinisk epikrise</summary>
                public const string A13_2 = "A13-2";

                /// <summary>Tverrfaglig behandlingsplan</summary>
                public const string B01_2 = "B01-2";
                /// <summary>Journalnotat</summary>
                public const string B02_2 = "B02-2";
                /// <summary>Poliklinisk notat</summary>
                public const string B03_2 = "B03-2";

                /// <summary>Medisinsk biokjemi</summary>
                public const string C01_2 = "C01-2";
                /// <summary>Blodbank og immunologi</summary>
                public const string C02_2 = "C02-2";
                /// <summary>Mikrobiologi, virologi og serologi</summary>
                public const string C03_2 = "C03-2";
                /// <summary>Patologi, histologi og cytologi</summary>
                public const string C04_2 = "C04-2";
                /// <summary>Klinisk farmakologi</summary>
                public const string C05_2 = "C05-2";
                /// <summary>Medisinsk genetikk</summary>
                public const string C06_2 = "C06-2";
                /// <summary>Allergiutredning</summary>
                public const string C07_2 = "C07-2";

                /// <summary>Hjerte og kretsløp</summary>
                public const string D01_2 = "D01-2";
                /// <summary>Lunge</summary>
                public const string D02_2 = "D02-2";
                /// <summary>Fordøyelse</summary>
                public const string D03_2 = "D03-2";
                /// <summary>Urinveier</summary>
                public const string D04_2 = "D04-2";
                /// <summary>Gyn/Reproduksjon</summary>
                public const string D05_2 = "D05-2";
                /// <summary>Nervesystemet</summary>
                public const string D06_2 = "D06-2";
                /// <summary>Ledd/ ben/ skjelett</summary>
                public const string D07_2 = "D07-2";
                /// <summary>ØNH</summary>
                public const string D08_2 = "D08-2";
                /// <summary>Øye</summary>
                public const string D09_2 = "D09-2";
                /// <summary>Hud</summary>
                public const string D10_2 = "D10-2";
                /// <summary>Endokrinologi</summary>
                public const string D11_2 = "D11-2";
                /// <summary>Metabolisme</summary>
                public const string D12_2 = "D12-2";
                /// <summary>Beinmargsutstryk</summary>
                public const string D13_2 = "D13-2";

                /// <summary>Bildediagnostiske svar</summary>
                public const string E01_2 = "E01-2";
                /// <summary>Foto og film</summary>
                public const string E02_2 = "E02-2";

                /// <summary>Kurve</summary>
                public const string F01_2 = "F01-2";
                /// <summary>Anestesi- og opr. Rapporter</summary>
                public const string F02_2 = "F02-2";
                /// <summary>Intensiv/postoperativ observasjon</summary>
                public const string F03_2 = "F03-2";
                /// <summary>Svangerskap og fødsel</summary>
                public const string F04_2 = "F04-2";
                /// <summary>Diabetes/ endokrinologi</summary>
                public const string F05_2 = "F05-2";
                /// <summary>Onkologi/ hematologi</summary>
                public const string F06_2 = "F06-2";
                /// <summary>Nyre/ dialyse</summary>
                public const string F07_2 = "F07-2";
                /// <summary>Smertebehandling</summary>
                public const string F08_2 = "F08-2";
                /// <summary>Ambulansejournal</summary>
                public const string F09_2 = "F09-2";
                /// <summary>Transplantasjon</summary>
                public const string F10_2 = "F10-2";

                /// <summary>Henvisninger</summary>
                public const string I01_2 = "I01-2";
                /// <summary>Brev</summary>
                public const string I02_2 = "I02-2";

                /// <summary>Sykmeldinger og trygdesaker</summary>
                public const string J01_2 = "J01-2";
                /// <summary>Legeerklæring om dødsfall</summary>
                public const string J02_2 = "J02-2";

                /// <summary>Tester</summary>
                public const string S01_2 = "S01-2";
                /// <summary>Systematiserte diagnostiske intervju</summary>
                public const string S02_2 = "S02-2";
                /// <summary>Voldsrisikovurdering</summary>
                public const string S03_2 = "S03-2";
            }

            public static class CategoryCode_9602
            {
                public const string System = "2.16.578.1.12.4.1.1.9602";

                /// <summary>Epikriser og sammenfatninger</summary>
                public const string A00_1 = "A00-1";

                /// <summary>Kontinuerlig/løpende journal</summary>
                public const string B00_1 = "B00-1";

                /// <summary>Prøvesvar, vev og væsker</summary>
                public const string C00_1 = "C00-1";

                /// <summary>Organfunksjon</summary>
                public const string D00_1 = "D00-1";

                /// <summary>Bildediagnostikk</summary>
                public const string E00_1 = "E00-1";

                /// <summary>Kurve, observasjon og behandling</summary>
                public const string F00_1 = "F00-1";

                /// <summary>Korrespondanse</summary>
                public const string I00_1 = "I00-1";

                /// <summary>Attester, melding og erklæringer</summary>
                public const string J00_1 = "J00-1";

                /// <summary>Test og scoring</summary>
                public const string S00_1 = "S00-1";
            }

            public static class ConfidentialityCode_9603
            {
                public const string System = "2.16.578.1.12.4.1.1.9603";

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
        }

        public static class Hl7
        {
            public static class ConfidentialityCode
            {
                public const string System = "2.16.840.1.113883.5.25";

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

            public static class Lifecycle
            {
                public const string IsoHealthRecordLifecycleEvent = "http://terminology.hl7.org/CodeSystem/iso-21089-lifecycle";
            }

            public static class AuditEventId
            {
                public const string System = "2.16.840.1.113883.4.642.3.462";
            }

            public static class PurposeOfUse
            {
                public const string System = "2.16.840.1.113883.1.11.20448";
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
                public const string System = "1.0.14265.1";
                public const string ClinicalCare_1 = "1";
                public const string EmergencyCare_2 = "2";
                public const string Management_5 = "5";
                public const string SubjectOfCare_13 = "13";
            }
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
    public static class Urn
    {
        public static class Custom
        {
            public const string BaseUrn = "urn:no:nhn:xcads:";
            public const string DocumentEntryPatientIdentifier = BaseUrn + "document:patient-identifier";
            public const string AdhocQueryPatientIdentifier = BaseUrn + "adhocquery:patient-identifier";
            public const string DocumentUniqueId = BaseUrn + "document:uniqueid";
            public const string RepositoryUniqueId = BaseUrn + "document:repositoryuniqueid";
            public const string HomeCommunityId = BaseUrn + "document:homecommunityid";
            public const string SamlNameId = BaseUrn + "saml:nameid";
            public const string AppliesTo = BaseUrn + "xacml:appliesto";
            public const string UnknownAttribute = BaseUrn + "xacml:unknownattribute";
            public const string UnknownPatientIdentifier = BaseUrn + "unknown-patient-identifier";
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

    public static ComprehensiveCodeSystem GetAsComprehensiveCodesystem(this Type type, Func<string, bool>? filter = null)
    {
        var codeSystem = type.GetAsKeyValuePair();

        var system = codeSystem.First(v => v.Key.Equals("system", StringComparison.OrdinalIgnoreCase)).Value;
        var values = codeSystem.Where(v => !v.Key.Equals("system", StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).ToArray();

        return new(system, values);
    }

    public static string[] GetAsStringList(this Type type, Func<string, bool> filter)
    {
        return type.GetAsStringList().Where(filter).ToArray();
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

        return constants.ToArray();
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
