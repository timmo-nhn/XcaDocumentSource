using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace XcaXds.WebService.Models.Custom;

public class ForbiddenOperationOutcome
{
    public static CustomContentResult Create(OperationOutcome operationOutcome)
    {
        return new CustomContentResult(operationOutcome.ToJson(), StatusCodes.Status403Forbidden);
    }
}
