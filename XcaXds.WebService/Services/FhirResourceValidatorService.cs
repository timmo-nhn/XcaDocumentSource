using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Hl7.FhirPath;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;


namespace XcaXds.Commons.DataManipulators.Fhir;

public class FhirResourceValidatorService
{
    private readonly ILogger<FhirResourceValidatorService> _logger;
    private readonly ApplicationConfig _appConfig;

    public FhirResourceValidatorService(ILogger<FhirResourceValidatorService> logger, ApplicationConfig appConfig)
    {
        _logger = logger;
        _appConfig = appConfig;

        AllowedPatientOids.Add(_appConfig.HomeCommunityId);
    }

    private static readonly HashSet<string> AllowedOrganizationOids =
    [
        Constants.Oid.Brreg,
        Constants.Oid.ReshId
    ];

    private static readonly HashSet<string> AllowedPractitionerOids =
    [
        Constants.Oid.Fnr,
        Constants.Oid.Hpr,
    ];

    private static readonly HashSet<string> AllowedPatientOids =
    [
        Constants.Oid.Fnr,
        Constants.Oid.Dnr,
        Constants.Oid.Hnr
    ];

    private static readonly HashSet<string> AllowedFacilityTypes =
    [
        Constants.CodeSystems.Volven.FacilityType
    ];

    private static readonly HashSet<KeyValueEntry> AllowedConfidentialityCodes = ConstantsExtensions.GetAsKeyValuePair(typeof(Constants.CodeSystems.Volven.ConfidentialityCode)).ToHashSet();

    public OperationOutcome ValidateFhirResource(Resource inputResource)
    {
        if (inputResource is Bundle fhirBundle)
        {
            return ValidateFhirBundle(fhirBundle);
        }

        return new OperationOutcome
        {
            Issue = new List<OperationOutcome.IssueComponent>
            {
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Invalid,
                    Diagnostics = $"Unsupported resource type: {inputResource.TypeName}"
                }
            }
        };
    }

    private OperationOutcome ValidateFhirBundle(Bundle fhirBundle)
    {
        var operationOutcome = new OperationOutcome();
        var identifiers = fhirBundle.Entry;

        ValidateOrganizations(operationOutcome, fhirBundle);
        ValidatePatients(operationOutcome, fhirBundle);
        ValidatePractitioners(operationOutcome, fhirBundle);
        ValidateCodeableConcepts(operationOutcome, fhirBundle);

        return operationOutcome;
    }

    private void ValidateCodeableConcepts(OperationOutcome outcome, Bundle bundle)
    {
        var codeableConcepts = FindDescendantResources(bundle, "CodeableConcept");

        var facilityType = codeableConcepts.Where(cc => cc.Name == "facilityType").ToArray();
        ValidateIdentifiers(outcome, codeableConcepts, AllowedFacilityTypes, "facilityType");

        var practiceSettings = codeableConcepts.Where(cc => cc.Name == "practiceSettings").ToArray();
        ValidateIdentifiers(outcome, codeableConcepts, AllowedFacilityTypes, "practiceSettings");
    }

    private void ValidatePractitioners(OperationOutcome outcome, Bundle bundle)
    {
        var orgs = FindDescendantResources(bundle, "Practitioner");
        ValidateIdentifiers(outcome, orgs, AllowedPractitionerOids, "Practitioner");
    }

    private static void ValidateOrganizations(OperationOutcome outcome,Bundle bundle)
    {
        var orgs = FindDescendantResources(bundle, "Organization");
        ValidateIdentifiers(outcome,orgs,AllowedOrganizationOids,"Organization");
    }

    private static void ValidatePatients(OperationOutcome outcome, Bundle bundle)
    {
        var patients = FindDescendantResources(bundle, "Patient");
        ValidateIdentifiers(outcome,patients,AllowedPatientOids,"Patient");
    }

    private static void ValidateIdentifiers(OperationOutcome outcome,IEnumerable<ITypedElement> resources,HashSet<string> allowedSystems,string resourceName)
    {
        foreach (var resourceElement in resources)
        {
            if (resourceElement.ToPoco() is not Resource resource)
                continue;

            var identifiers = GetIdentifiers(resource);

            if (identifiers == null)
                continue;

            for (int i = 0; i < identifiers.Count; i++)
            {
                var identifier = identifiers[i];

                if (string.IsNullOrWhiteSpace(identifier.System))
                    continue;

                if (allowedSystems.Contains(identifier.System.NoUrn()))
                    continue;

                outcome.AddIssue(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Warning,
                    Code = OperationOutcome.IssueType.CodeInvalid,
                    Diagnostics =
                        $"Unknown OID for {resourceName} (Id={identifier.Value}, OID={identifier.System})",
                    Location =
                    [
                        $"{resourceElement.Location}.identifier[{i}]"
                    ]
                });
            }
        }
    }

    private static IList<Identifier>? GetIdentifiers(Resource resource) =>
    resource switch
    {
        Organization o => o.Identifier,
        Patient p => p.Identifier,
        Practitioner pr => pr.Identifier,
        _ => null
    };

    private static IEnumerable<ITypedElement> FindDescendantResources(Bundle bundle,string resourceType)
    {
#pragma warning disable SDK0001
        var root = bundle.ToTypedElement();
#pragma warning restore SDK0001

        return root.Select($"descendants().where($this is {resourceType})");
    }
}