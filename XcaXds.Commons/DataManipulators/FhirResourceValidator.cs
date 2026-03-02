using Hl7.Fhir.Model;

namespace XcaXds.WebService.Controllers;

public static class FhirResourceValidator
{
    public static OperationOutcome ValidateFhirResource(Resource inputResource)
    {
        if (inputResource is Bundle fhirBundle)
        {
            return ValidateFhirBundle(fhirBundle);
        }

        throw new NotImplementedException();
    }

    private static OperationOutcome ValidateFhirBundle(Bundle fhirBundle)
    {

        throw new NotImplementedException();
    }
}