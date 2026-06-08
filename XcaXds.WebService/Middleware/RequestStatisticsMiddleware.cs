using Hl7.Fhir.Model;
using System.Diagnostics;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces.Statistics;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.Statistics;
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
        // Important: rewind returned stream, or readers will see empty content.
        if (responseStream.CanSeek)
            responseStream.Seek(0, SeekOrigin.Begin);

        var streamCopy = new MemoryStream();
        await responseStream.CopyToAsync(streamCopy);

        streamCopy.Seek(0, SeekOrigin.Begin);

        if (responseStream.CanSeek)
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
        var requestContentType = context.Request.ContentType;
        var responseContentType = context.Response.ContentType;

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
            RequestContentType = requestContentType,
            ResponseContentType = responseContentType,
            Method = method,
            StatusCode = statusCode,
            RelatedDocumentEntries = GetDocumentEntriesRelatedToRequest(context, requestBody, requestType)
        };
    }

    private static RequestAndFieldRequestType GetRequestTypeFromContext(PathString path, string method)
    {
        var isfhirRequest = path.ToString().StartsWith("/R4/fhir");
        var isSoapRequest = path.ToString() is { } item && (item.StartsWith("/XCA/services") || item.StartsWith("/Registry/services") || item.StartsWith("/Repository/services"));

        return (isfhirRequest, isSoapRequest, method) switch
        {
            (true, _, "POST") => RequestAndFieldRequestType.FhirProvideBundle,
            (true, _, _) => RequestAndFieldRequestType.FhirUrlBasedRequest,
            (_, true, "POST") => RequestAndFieldRequestType.SoapEnvelope,
            _ => RequestAndFieldRequestType.Unknown
        };
    }

    private static DocumentEntryDto[]? GetDocumentEntriesRelatedToRequest(HttpContext context, Stream? requestBody, RequestAndFieldRequestType requestType)
    {
        var fhirBundle = requestType == RequestAndFieldRequestType.FhirProvideBundle ? Hl7FhirExtensions.GetResourceFromStream(requestBody) as Bundle : null;
        var documentEntriesFromBundle = GetDocumentEntriesFromBundle(fhirBundle);

        var deletedRegistryObjects = (context.Items.TryGetValue("deletedRegistryObjects", out var entry) ? entry as List<DocumentEntryDto> : []) ?? [];

        if (documentEntriesFromBundle != null)
        {
            deletedRegistryObjects.Add(documentEntriesFromBundle);
        }

        return deletedRegistryObjects.Where(de => de != null).OfType<DocumentEntryDto>().ToArray();
    }

    private static DocumentEntryDto? GetDocumentEntriesFromBundle(Bundle? fhirBundle)
    {
        if (fhirBundle == null) return null;

        var patient = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<Patient>()
            .FirstOrDefault();

        var documentReferences = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<DocumentReference>()
            .FirstOrDefault();

        var fhirBinaries = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<Binary>()
            .FirstOrDefault();

        var extrinsicObject = FhirToXdsTransformer.ConvertDocumentReferenceToExtrinsicObject(patient, documentReferences, fhirBinaries);
        var documentEntry = RegistryMetadataTransformer.TransformRegistryObjectToRegistryObjectDto(extrinsicObject.Value) as DocumentEntryDto;

        return documentEntry;
    }

    private static bool IsMiddlewareEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<ExportsAtnaAuditLogAttribute>();
        return enforceAttr?.Enabled == true;
    }
}