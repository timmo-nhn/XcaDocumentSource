using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Hl7.FhirPath;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;


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
        Constants.Oid.Hpr,
    ];

    private static readonly HashSet<string> AllowedPatientOids =
    [
        Constants.Oid.Fnr,
        Constants.Oid.Dnr,
        Constants.Oid.Hnr
    ];

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

        return operationOutcome;
    }

    private static void ValidateOrganizations(OperationOutcome outcome,Bundle bundle)
    {
        var orgs = FindResources(bundle, "Organization");

        ValidateIdentifiers(outcome,orgs,AllowedOrganizationOids,"Organization");
    }

    private static void ValidatePatients(OperationOutcome outcome, Bundle bundle)
    {
        var patients = FindResources(bundle, "Patient");

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

    private static IEnumerable<Hl7.Fhir.ElementModel.ITypedElement> FindResources(Bundle bundle,string resourceType)
    {
#pragma warning disable SDK0001
        var root = bundle.ToTypedElement();
#pragma warning restore SDK0001

        return root.Select($"descendants().where($this is {resourceType})");
    }
}