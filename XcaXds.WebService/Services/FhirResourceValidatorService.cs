using Firely.Fhir.Packages;
using Firely.Fhir.Validation;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;
using Hl7.Fhir.Support;
using Hl7.FhirPath;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;

namespace XcaXds.Commons.DataManipulators.Fhir;

public class FhirResourceValidatorService
{
    private readonly ILogger<FhirResourceValidatorService> _logger;
    private readonly ApplicationConfig _appConfig;

    private Validator _validator;

    public FhirResourceValidatorService(ILogger<FhirResourceValidatorService> logger, ApplicationConfig appConfig)
    {
        _logger = logger;
        _appConfig = appConfig;

        _validator = InitValidator();

        AllowedPatientOids.Add(_appConfig.HomeCommunityId);
    }

    public OperationOutcome ValidateFhirResource(Resource inputResource)
    {
        var outcome = new OperationOutcome();

        switch (inputResource)
        {
            case Bundle fhirBundle:
                var bundleOutcome = ValidateFhirBundle(fhirBundle);
                outcome.Issue.AddRange(bundleOutcome.Issue);
                break;

            default:
                outcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Invalid,
                    Diagnostics = $"Unsupported resource type: {inputResource.TypeName}"
                });
                break;
        }

        var oo = _validator.Validate(inputResource);

        outcome.Issue.AddRange(oo.Issue);
        return outcome;
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
        var codeableConcepts = FindDescendantResources(bundle, "CodeableConcept", "Coding", "Attachment").ToArray();

        ValidateIdentifiers(outcome, codeableConcepts, "facilityType", AllowedFacilityTypes);
        ValidateIdentifiers(outcome, codeableConcepts, "practiceSetting", AllowedPracticeSettings);
        ValidateIdentifiers(outcome, codeableConcepts, "securityLabel", AllowedConfidentialityCodes);
        ValidateIdentifiers(outcome, codeableConcepts, "type", AllowedTypeCodes);
        ValidateIdentifiers(outcome, codeableConcepts, "category", AllowedCategoryCodes);
        ValidateIdentifiers(outcome, codeableConcepts, "format", AllowedFormatCodes);
        ValidateIdentifiers(outcome, codeableConcepts, "attachment", _appConfig.HomeCommunityId);
    }

    private void ValidatePractitioners(OperationOutcome outcome, Bundle bundle)
    {
        var orgs = FindDescendantResources(bundle, "Practitioner");
        ValidateIdentifiers(outcome, orgs, "Practitioner", AllowedPractitionerOids);
    }

    private static void ValidateOrganizations(OperationOutcome outcome, Bundle bundle)
    {
        var orgs = FindDescendantResources(bundle, "Organization");
        ValidateIdentifiers(outcome, orgs, "Organization", AllowedOrganizationOids);
    }

    private static void ValidatePatients(OperationOutcome outcome, Bundle bundle)
    {
        var patients = FindDescendantResources(bundle, "Patient");
        ValidateIdentifiers(outcome, patients, "Patient", AllowedPatientOids);
    }

    private static void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, string allowedSystems)
    {
        ValidateIdentifiers(outcome, resources, resourceName, [allowedSystems]);
    }

    private static void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, HashSet<string> allowedSystems)
    {
        // Filter out unrelated resources
        resources = resources.Where(cc => cc.Name == resourceName).ToArray();

        foreach (var resourceElement in resources)
        {
            if (resourceElement.ToPoco() is not Base resource)
            {
                outcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Invalid,
                    Diagnostics = $"Unsupported resource type: {resourceElement.Name}",
                    Location = [resourceElement.Location]
                });

                continue;
            }

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

    private static IList<Identifier>? GetIdentifiers(Base resource)
    {
        return resource switch
        {
            Organization o => o.Identifier,
            Attachment a => IdentifierFromExtension(a),
            Patient p => p.Identifier,
            Practitioner pr => pr.Identifier,
            Coding cc => [new Identifier(cc.System, cc.Code)],
            CodeableConcept cc => cc.Coding.Select(cod => new Identifier(cod.System, cod.Code)).ToArray(),
            _ => null
        };
    }

    private static List<Identifier> IdentifierFromExtension(Attachment attachment)
    {
        return attachment.Extension.Select(ext => new Identifier(ext.Url, ext.ToCodings().FirstOrDefault()?.Code)).ToList();
    }

    private static IEnumerable<ITypedElement> FindDescendantResources(Bundle bundle, params string[] resourceTypes)
    {
#pragma warning disable SDK0001
        var root = bundle.ToTypedElement();
#pragma warning restore SDK0001

        var query = $"descendants().where({string.Join(" or ", resourceTypes.Select(rt => $"$this is {rt}"))})";
        return root.Select(query);
    }

    private Validator InitValidator()
    {
        var inspector = ModelInfo.ModelInspector;

        var packageSource = new FhirPackageSource(
            inspector,
            "https://packages.fhir.org",
            new[]
            {
            "hl7.fhir.r4.core#4.0.1",
            "ihe.iti.mhd#4.2.0"
            }
        );

        var resolver = new CachedResolver(packageSource);

        var terminologyService = new LocalTerminologyService(resolver);

        return new Validator(resolver, terminologyService, null, new ValidationSettings() { ConformanceResourceResolver = packageSource });
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

    private static readonly HashSet<string> AllowedPracticeSettings =
    [
        Constants.CodeSystems.Volven.PracticeSetting
    ];

    private static readonly HashSet<string> AllowedTypeCodes =
    [
        Constants.CodeSystems.Volven.TypeCode
    ];

    private static readonly HashSet<string> AllowedCategoryCodes =
    [
        Constants.CodeSystems.Volven.CategoryCode
    ];

    private static readonly HashSet<string> AllowedConfidentialityCodes =
    [
        Constants.CodeSystems.Volven.ConfidentialityCode.Oid,
        "http://terminology.hl7.org/CodeSystem/v3-Confidentiality"
    ];

    private static readonly HashSet<string> AllowedFormatCodes =
    [
        "http://www.kith.no/xmlstds/epikrise/2012-02-15",
        "formatCodes"
    ];
}