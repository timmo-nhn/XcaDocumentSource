using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;

namespace XcaXds.WebService.Middleware;

public class SoapServiceStatisticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SoapServiceStatisticsMiddleware> _logger;
    public static Channel<SoapEnvelopeAndFields> RawStatisticsOutputChannel = Channel.CreateUnbounded<SoapEnvelopeAndFields>();

    public SoapServiceStatisticsMiddleware(RequestDelegate next, ILogger<SoapServiceStatisticsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        // Previous middleware should already have enabled buffering
        // but do it here aswell just to be explicit about it
        context.Request.EnableBuffering();
        context.Request.Body.Position = 0;

        // Read the stream before invoking the next middleware,
        // because the context will dispose the request stream before
        // finishing the request.
        var requestBody = new StreamReader(context.Request.Body).ReadToEnd();

        await _next(context);

        sw.Stop();

        var soapEnvelope = await RequestHasSoapEnvelope(context, requestBody);
        if (soapEnvelope == null) return;

        
        var elapsedMs = sw.ElapsedMilliseconds;

        var path = context.Request.Path;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;

        var soapEnvelopeAndFields = new SoapEnvelopeAndFields
        {
            AccessTime = DateTime.UtcNow,
            SoapEnvelope = soapEnvelope,
            ElapsedMilliseconds = elapsedMs,
            Path = path,
            Method = method,
            StatusCode = statusCode
        };

        RawStatisticsOutputChannel.Writer.TryWrite(soapEnvelopeAndFields);
    }

    private async Task<SoapEnvelope?> RequestHasSoapEnvelope(HttpContext context, string? requestBody)
    {
        var contentType = context.Request.ContentType?.Split(";").FirstOrDefault();
        if (!contentType.IsAnyOf(Constants.MimeTypes.SoapXml, Constants.MimeTypes.MultipartRelated))
        {
            return null;
        }

        SoapEnvelope? requestEnvelope = null;

        var sxmls = new SoapXmlSerializer();

        if (contentType == Constants.MimeTypes.MultipartRelated)
        {
            requestEnvelope = await MultipartExtensions.ReadMultipartSoapMessage(context.Request.ContentType!, requestBody);
        }
        else
        {
            requestEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(requestBody);
        }


        return requestEnvelope;
    }
}
