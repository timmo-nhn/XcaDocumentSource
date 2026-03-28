using Firely.Fhir.Packages;
using Firely.Fhir.Validation;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;
using Hl7.Fhir.Support;
using Hl7.FhirPath;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;

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

        AllowedPatientOids.Add(new(_appConfig.HomeCommunityId));
        AllowedAttachments.Add(new("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId", [_appConfig.HomeCommunityId]));
    }

    public OperationOutcome ValidateFhirResource(Resource inputResource, bool useFirelyValidator = false)
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

        if (useFirelyValidator)
        {
            var oo = _validator.Validate(inputResource);
            outcome.Issue.AddRange(oo.Issue);
        }
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
        ValidateIdentifiers(outcome, codeableConcepts, "attachment", AllowedAttachments);
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

    private static void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, ComprehensiveCodeSystem allowedSystems)
    {
        ValidateIdentifiers(outcome, resources, resourceName, [allowedSystems]);
    }

    private static void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, IEnumerable<ComprehensiveCodeSystem> codeSystems)
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

                if (string.IsNullOrWhiteSpace(identifier.System) || string.IsNullOrWhiteSpace(identifier.Value))
                    continue;

                var systemsMatch = codeSystems.Systems().Contains(identifier.System.NoUrn());

                // If Values is empty, accept anything
                // The most psuedo-ternary-operatorial thing
                var valuesMatch = (codeSystems.Values(identifier.System) ?? [identifier.Value]).Contains(identifier.Value.NoUrn());

                if (valuesMatch && systemsMatch)
                    continue;

                outcome.AddIssue(new OperationOutcome.IssueComponent
                {
                    // We will allow unknown Systems with a warning, but if the system is known but the value in it isn't, it's an error.
                    Severity = systemsMatch && !valuesMatch ? OperationOutcome.IssueSeverity.Error : OperationOutcome.IssueSeverity.Warning,
                    Code = OperationOutcome.IssueType.CodeInvalid,
                    // Nested ternary for a nice diagnostics message
                    Diagnostics = $"Unknown {(valuesMatch ? "System" : systemsMatch ? "Value" : "System and Value")} for {resourceName} (Value={identifier.Value}, System={identifier.System})",
                    Location = [resourceElement.Location]
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

        var coreSource = FhirPackageSource.CreateCorePackageSource(
            inspector,
            FhirRelease.R4,
            "https://packages.simplifier.net") ?? throw new InvalidOperationException("Core Package Source could not be created");

        var iheSource = new FhirPackageSource(
            inspector,
            "https://packages.simplifier.net",
            ["ihe.iti.mhd@4.2.0"]);


        var resolver = new CachedResolver(
            new MultiResolver(coreSource, iheSource));

        var terminologyService = new LocalTerminologyService(resolver);

        return new Validator(resolver, terminologyService);
    }

    private static readonly List<ComprehensiveCodeSystem> AllowedOrganizationOids =
    [
        new (Constants.Oid.Brreg),
        new (Constants.Oid.ReshId)
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedPractitionerOids =
    [
        new(Constants.Oid.Fnr),
        new(Constants.Oid.Hpr),
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedPatientOids =
    [
        new(Constants.Oid.Fnr),
        new(Constants.Oid.Dnr),
        new(Constants.Oid.Hnr)
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedFacilityTypes =
    [
        typeof(Constants.CodeSystems.Volven.FacilityType_1303).GetAsComprehensiveCodesystem(),
        typeof(Constants.CodeSystems.Volven.FacilityType_1305).GetAsComprehensiveCodesystem()
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedPracticeSettings =
    [
        typeof(Constants.CodeSystems.Volven.PracticeSetting_8651).GetAsComprehensiveCodesystem(),
        typeof(Constants.CodeSystems.Volven.PracticeSetting_8653).GetAsComprehensiveCodesystem(),
        typeof(Constants.CodeSystems.Volven.PracticeSetting_8654).GetAsComprehensiveCodesystem(),
        typeof(Constants.CodeSystems.Volven.PracticeSetting_8655).GetAsComprehensiveCodesystem(),
        typeof(Constants.CodeSystems.Volven.PracticeSetting_8663).GetAsComprehensiveCodesystem()
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedTypeCodes =
    [
        typeof(Constants.CodeSystems.Volven.TypeCode_9602).GetAsComprehensiveCodesystem()
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedCategoryCodes =
    [
        typeof(Constants.CodeSystems.Volven.CategoryCode_9602).GetAsComprehensiveCodesystem()
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedConfidentialityCodes =
    [
        typeof(Constants.CodeSystems.Volven.ConfidentialityCode_9603).GetAsComprehensiveCodesystem(),
        typeof(Constants.CodeSystems.Hl7.ConfidentialityCode).GetAsComprehensiveCodesystem(),
        new("http://terminology.hl7.org/CodeSystem/v3-Confidentiality")
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedFormatCodes =
    [
        new("http://ihe.net/fhir/ihe.formatcode.fhir/CodeSystem/formatcode"),
        new("http://www.kith.no/xmlstds/epikrise/2012-02-15"),
        new("formatCodes")
    ];

    private static readonly List<ComprehensiveCodeSystem> AllowedAttachments =
    [
    ];
}