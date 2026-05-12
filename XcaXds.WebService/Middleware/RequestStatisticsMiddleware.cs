using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Diagnostics;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces.Statistics;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.WebService.Services;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.Middleware;

public class RequestStatisticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestStatisticsMiddleware> _logger;
    private readonly IStatisticsQueue _statisticsQueue;
    public RequestStatisticsMiddleware(RequestDelegate next, ILogger<RequestStatisticsMiddleware> logger, IStatisticsQueue statisticsQueue)
    {
        _next = next;
        _logger = logger;
        _statisticsQueue = statisticsQueue;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var sw = Stopwatch.StartNew();

        var originalResponseBody = httpContext.Response.Body;

        var requestBody = await CopyStreamAsync(httpContext.Request.Body);

        await using var responseBuffer = new MemoryStream();
        httpContext.Response.Body = responseBuffer;

        try
        {
            await _next(httpContext);
        }
        finally
        {
            httpContext.Response.Body = originalResponseBody;
        }

        sw.Stop();


        var responseBody = await CopyStreamAsync(responseBuffer);
        await CopyResponseToOriginalStreamAsync(responseBuffer, originalResponseBody);

        if (!IsMiddlewareEnabledForRequestEndpoint(httpContext)) return;

        var requestAndFields = GetRequestAndFieldsFromRequestResponse(sw, httpContext, requestBody, responseBody, httpContext.Request.Headers.Authorization);

        if (!_statisticsQueue.Channel.Writer.TryWrite(requestAndFields))
            throw new InvalidOperationException("statistics was not exported");
    }

    private static async Task<Stream> CopyStreamAsync(Stream responseStream)
    {
        responseStream.Seek(0, SeekOrigin.Begin);
    
        var streamCopy = new MemoryStream(); 
        await responseStream.CopyToAsync(streamCopy);
        
        responseStream.Seek(0, SeekOrigin.Begin);

        return streamCopy;
    }

    private static async Task CopyResponseToOriginalStreamAsync(Stream responseBuffer, Stream originalResponseStream)
    {
        responseBuffer.Seek(0, SeekOrigin.Begin);
        await responseBuffer.CopyToAsync(originalResponseStream);
    }

    private StatisticsRequestAndFields GetRequestAndFieldsFromRequestResponse(Stopwatch sw, HttpContext context, Stream? requestBody, Stream? responseBody, string? jwt)
    {
        var elapsedMs = sw.ElapsedMilliseconds;
        var sessionId = context.TraceIdentifier;
        var path = context.Request.Path;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;
        var contentType = context.Request.ContentType;
        
        
        var requestType = GetRequestTypeFromContext(path, method);

        return new StatisticsRequestAndFields()
        {
            RequestType = requestType,
            AccessTime = DateTime.UtcNow,
            SessionId = sessionId,
            RequestBody = requestBody,
            ResponseBody = responseBody,
            JwtToken = jwt,
            ElapsedMilliseconds = elapsedMs,
            Path = path,
            ContentType = contentType,
            Method = method,
            StatusCode = statusCode,
            RelatedDocumentEntries = GetDocumentEntriesRelatedToRequest(context, requestType == RequestAndFieldRequestType.FhirProvideBundle ? Hl7FhirExtensions.GetResourceFromStream(requestBody) as Bundle : null)
        };
    }

    private RequestAndFieldRequestType GetRequestTypeFromContext(PathString path, string method)
    {
        var isfhirRequest = path.ToString().StartsWith("/R4/fhir");
        var isSoapRequest = path.ToString().StartsWith("/XCA/services");

        return (isfhirRequest, isSoapRequest, method) switch
        {
            (true, _, "POST") => RequestAndFieldRequestType.FhirProvideBundle,
            (true, _, _) => RequestAndFieldRequestType.FhirUrlBasedRequest,
            (_, true, "POST") => RequestAndFieldRequestType.SoapEnvelope,
            _ => RequestAndFieldRequestType.Unknown
        };
    }

    private DocumentEntryDto[]? GetDocumentEntriesRelatedToRequest(HttpContext context, Bundle? fhirBundleRequest)
    {
        DocumentEntryDto?[]? deletedDocumentEntry = context.Items.TryGetValue("deletedEntry", out var entry) ? [entry as DocumentEntryDto] : [null];

        return deletedDocumentEntry.OfType<DocumentEntryDto>().ToArray();
    }
    private bool IsMiddlewareEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<ExportsAtnaAuditLogAttribute>();
        return enforceAttr?.Enabled == true;
    }

}
