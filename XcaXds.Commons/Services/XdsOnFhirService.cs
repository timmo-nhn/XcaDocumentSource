
using System.Globalization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public class XdsOnFhirService
{
    private readonly ILogger<XdsOnFhirService> _logger;
    public XdsOnFhirService(ILogger<XdsOnFhirService> logger)
    {
        _logger = logger;
    }

    public AdhocQueryRequest ConvertIti67ToIti18AdhocQuery(MhdDocumentRequest documentRequest)
    {
        var adhocQueryRequest = new AdhocQueryRequest();
        var adhocQuery = new AdhocQueryType();

        if (!string.IsNullOrWhiteSpace(documentRequest.Patient))
        {
            var patientCx = Hl7Object.Parse<CX>(documentRequest.Patient, '|');

            var patientOid = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(documentRequest.Patient);

            if (patientOid != null)
            {
                patientCx.AssigningAuthority ??= new()
                {
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                    UniversalId = patientOid.AssigningAuthority.UniversalId
                };
            }

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId, [patientCx.Serialize()]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Creation))
        {
            var documentCreationTimeRange = Hl7FhirExtensions.GetDateTimeRangeFromDateParameters(documentRequest.Creation);

            if (documentCreationTimeRange.Start.HasValue)
            {
                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.CreationTimeFrom, [documentCreationTimeRange.Start.Value.ToString("O")]);
            }

            if (documentCreationTimeRange.End.HasValue)
            {
                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.CreationTimeTo, [documentCreationTimeRange.End.Value.ToString("O")]);
            }
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.AuthorFamily))
        {
            adhocQuery.UpdateSlot(Constants.Xds.QueryParameters.FindDocuments.AuthorPerson, [documentRequest.AuthorFamily]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.AuthorGiven))
        {
            adhocQuery.UpdateSlot(Constants.Xds.QueryParameters.FindDocuments.AuthorPerson, [documentRequest.AuthorGiven]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Status))
        {
            var ebRimStatus = documentRequest.Status switch
            {
                "current" => Constants.Xds.StatusValues.Approved,
                "superseded" => Constants.Xds.StatusValues.Deprecated,
                _ => Constants.Xds.StatusValues.Approved
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.Status, [ebRimStatus]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Category))
        {
            var classCodeCx = Hl7Object.Parse<CX>(documentRequest.Category, '|');

            classCodeCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.CodeSystems.Volven.DocumentType
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.ClassCode, [classCodeCx.Serialize()]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Type))
        {
            var classCodeCx = Hl7Object.Parse<CX>(documentRequest.Category, '|');

            classCodeCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.CodeSystems.Volven.DocumentType
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.ClassCode, [classCodeCx.Serialize()]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Event))
        {
            var eventCodeCx = Hl7Object.Parse<CX>(documentRequest.Event, '|');

            eventCodeCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.CodeSystems.Volven.EventCode
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.EventCodeList, [eventCodeCx.Serialize()]);
        }



        adhocQueryRequest.AdhocQuery = adhocQuery;

        return adhocQueryRequest;
    }

    public Bundle TransformRegistryObjectsToBundle(IdentifiableType[] registryObjectList)
    {
        // Create a Bundle with DocumentReference resources and return it as the response
        // See example here https://profiles.ihe.net/ITI/MHD/Bundle-Bundle-FindDocumentReferences.json
        var bundle = new Bundle
        {
            Id = Guid.NewGuid().ToString(),
            Meta = new Meta()
            {
                VersionId = "1",
                LastUpdatedElement = Instant.Now(),
                Profile = new List<string>() { "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.FindDocumentReferencesComprehensiveResponseMessage" },
            },
            Type = Bundle.BundleType.Searchset,
            Total = registryObjectList.Length,
            Timestamp = DateTime.UtcNow,
            Entry = new List<Bundle.EntryComponent>()
        };

        bundle.Entry.Add(new Bundle.EntryComponent()
        {
            Resource = new()
            {
                TypeName = ResourceType.DocumentReference,
            }
        });

        return bundle;
    }

}
