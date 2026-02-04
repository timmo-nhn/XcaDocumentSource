using XcaXds.Commons.Commons;
using System.Net;
using System.Text.Json;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyWriter;

public class JsonDenyResponseStrategy : IPepDenyResponseStrategy
{
    public bool CanHandle(string? contentType, PolicyInputResult input) => true;

    public async Task WriteAsync(HttpContext context, PolicyInputResult input, ApplicationConfig appConfig, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentType = Constants.MimeTypes.Json;

        var resp = new RestfulApiResponse(false, message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(resp, Constants.JsonDefaultOptions.DefaultSettings));
    }

    public string[] GetAcceptedContentTypes()
    {
        return [
            
        ];
    }
}
