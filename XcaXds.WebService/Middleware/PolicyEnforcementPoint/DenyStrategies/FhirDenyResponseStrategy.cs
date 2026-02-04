using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Net;
using XcaXds.Commons.Commons;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyWriter
{
    public class FhirDenyResponseStrategy : IPepDenyResponseStrategy
    {
        public bool CanHandle(string? contentType, PolicyInputResult input) =>
            GetAcceptedContentTypes().Contains(contentType);


        public async Task WriteAsync(HttpContext context, PolicyInputResult input, ApplicationConfig appConfig, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = Constants.MimeTypes.FhirJson;

            var outcome = OperationOutcome.ForMessage(
                message,
                OperationOutcome.IssueType.Forbidden,
                OperationOutcome.IssueSeverity.Error);

            var serializer = new FhirJsonSerializer();
            await context.Response.WriteAsync(serializer.SerializeToString(outcome, true));
        }

        public string[] GetAcceptedContentTypes()
        {
            return [
                Constants.MimeTypes.FhirJson,
            ];
        }

    }
}
