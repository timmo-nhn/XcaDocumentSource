
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;

namespace XcaXds.WebService.Models.Custom;
public class BadRequestOperationOutcome
{
    public static CustomContentResult Create(OperationOutcome operationOutcome)
    {
        return new CustomContentResult(operationOutcome.ToJson(), StatusCodes.Status400BadRequest);
    }
}
