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
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Extensions;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;

namespace XcaXds.WebService.Services.Fhir;

public class FhirResourceValidatorService
{
    private readonly ILogger<FhirResourceValidatorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly BusinessLogicFiltersRegistry _businessLogicFiltersRegistry;

    private Validator _validator;

    public FhirResourceValidatorService(ILogger<FhirResourceValidatorService> logger, ApplicationConfig appConfig, BusinessLogicFiltersRegistry businessLogicFiltersRegistry)
    {
        _logger = logger;
        _appConfig = appConfig;
        _businessLogicFiltersRegistry = businessLogicFiltersRegistry;

        _validator = InitValidator();

        //_businessLogicFiltersRegistry.GetAllowedPatientOids().Add(new(_appConfig.HomeCommunityId));
        //_businessLogicFiltersRegistry.GetAllowedAttachments().Add(new("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId", [new(_appConfig.HomeCommunityId)]));
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

        ValidateIdentifiers(outcome, codeableConcepts, "facilityType", _businessLogicFiltersRegistry.GetAllowedFacilityTypes());
        ValidateIdentifiers(outcome, codeableConcepts, "practiceSetting", _businessLogicFiltersRegistry.GetAllowedPracticeSettings());
        ValidateIdentifiers(outcome, codeableConcepts, "securityLabel", _businessLogicFiltersRegistry.GetAllowedConfidentialityCodes());
        ValidateIdentifiers(outcome, codeableConcepts, "type", _businessLogicFiltersRegistry.GetAllowedTypeCodes());
        ValidateIdentifiers(outcome, codeableConcepts, "type", "contained", _businessLogicFiltersRegistry.GetAllowedOrganizationSystems());
        ValidateIdentifiers(outcome, codeableConcepts, "category", _businessLogicFiltersRegistry.GetAllowedClassCodes());
        ValidateIdentifiers(outcome, codeableConcepts, "format", _businessLogicFiltersRegistry.GetAllowedFormatCodes());
        ValidateIdentifiers(outcome, codeableConcepts, "attachment", _businessLogicFiltersRegistry.GetAllowedAttachments());
    }

    private void ValidatePractitioners(OperationOutcome outcome, Bundle bundle)
    {
        var orgs = FindDescendantResources(bundle, "Practitioner");
        ValidateIdentifiers(outcome, orgs, "Practitioner", _businessLogicFiltersRegistry.GetAllowedPractitionerSystems());
    }

    private void ValidateOrganizations(OperationOutcome outcome, Bundle bundle)
    {
        var orgs = FindDescendantResources(bundle, "Organization");
        ValidateIdentifiers(outcome, orgs, "Organization", _businessLogicFiltersRegistry.GetAllowedOrganizationSystems());
    }

    private void ValidatePatients(OperationOutcome outcome, Bundle bundle)
    {
        var patients = FindDescendantResources(bundle, "Patient");
        ValidateIdentifiers(outcome, patients, "Patient", _businessLogicFiltersRegistry.GetAllowedPatientSystems());
    }

    private void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, ComprehensiveCodeSystem allowedSystem)
    {
        ValidateIdentifiers(outcome, resources, resourceName, [allowedSystem]);
    }

    private void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, IEnumerable<ComprehensiveCodeSystem> codeSystems)
    {
        ValidateIdentifiers(outcome, resources, resourceName, "resource", codeSystems);
    }

    private void ValidateIdentifiers(OperationOutcome outcome, IEnumerable<ITypedElement> resources, string resourceName, string parentResourceName, IEnumerable<ComprehensiveCodeSystem> codeSystems)
    {
        // Filter out unrelated resources
        resources = resources
            .Where(cc =>
                string.Equals(cc.Name, resourceName, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(cc.GetParentName(), parentResourceName, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

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

                var systemsMatch = codeSystems.SystemOids().Contains(identifier.System.NoUrn()) || codeSystems.SystemUrls().Contains(identifier.System.NoUrn());

                // If Values is empty, accept anything
                var valuesMatch = codeSystems.Values(identifier.System)?.Select(v => v.Value).Contains(identifier.Value.NoUrn()) == true;

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
}