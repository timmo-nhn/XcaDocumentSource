using Hl7.Fhir.Model.CdsHooks;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Middleware;

public class SessionIdTraceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionIdTraceMiddleware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;

    public SessionIdTraceMiddleware(RequestDelegate next, ILogger<SessionIdTraceMiddleware> logger, ApplicationConfig xdsConfig, IWebHostEnvironment env)
    {
        _logger = logger;
        _next = next;
        _xdsConfig = xdsConfig;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var contentType = httpContext.Request.ContentType?.Split(";").First();

        httpContext.Request.EnableBuffering();

        var requestBody = await httpContext.Request.GetHttpRequestBodyAsStringAsync();

        if (httpContext.Request.ContentType != null && (httpContext.Request.ContentType.Contains(Constants.MimeTypes.XopXml) || httpContext.Request.ContentType.Contains(Constants.MimeTypes.MultipartRelated)))
        {
            requestBody = await HttpRequestResponseExtensions.ReadMultipartContentFromRequest(httpContext);
        }


        switch (contentType)
        {
            case Constants.MimeTypes.XopXml:
            case Constants.MimeTypes.MultipartRelated:
            case Constants.MimeTypes.SoapXml:

                var sxmls = new SoapXmlSerializer();

                var soapEnvelope = sxmls.DeserializeSoapMessage<SoapEnvelope>(requestBody);

                httpContext.TraceIdentifier = soapEnvelope.Header.MessageId ?? Guid.NewGuid().ToString();

                break;

            default:
                httpContext.TraceIdentifier = Guid.NewGuid().ToString();
                break;
        }

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Trace identifier set: {httpContext.TraceIdentifier}");
        await _next(httpContext);
    }
}