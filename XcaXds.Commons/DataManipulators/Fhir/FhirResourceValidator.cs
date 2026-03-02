using Hl7.Fhir.Model;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.DataManipulators.Fhir;

public static class FhirResourceValidator
{
    private static readonly HashSet<string> AllowedOrganizationOids =
    [
        Constants.Oid.Brreg,
        Constants.Oid.ReshId
    ];

    public static OperationOutcome ValidateFhirResource(Resource inputResource)
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

    private static OperationOutcome ValidateFhirBundle(Bundle fhirBundle)
    {
        var operationOutcome = new OperationOutcome();
        var identifiers = fhirBundle.Entry;

        ValidateOrganization(operationOutcome, fhirBundle);

        throw new NotImplementedException();
    }

    private static void ValidateOrganization(OperationOutcome operationOutcome, Bundle fhirBundle)
    {
        throw new NotImplementedException();
    }
}