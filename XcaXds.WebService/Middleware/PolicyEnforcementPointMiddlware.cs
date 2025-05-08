using Microsoft.IdentityModel.Tokens.Saml2;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Middleware;

public class PolicyEnforcementPointMiddlware
{
    private readonly RequestDelegate _next;

    public PolicyEnforcementPointMiddlware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        Console.WriteLine("ENFORCE!");
        httpContext.Request.EnableBuffering(); // Allows multiple reads
        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();

        httpContext.Request.Body.Position = 0; // Reset stream position for next reader

        var soapEnvelope = new XmlDocument();
        soapEnvelope.LoadXml(bodyContent);

        var assertion = soapEnvelope.GetElementsByTagName("saml:Assertion");

        if (assertion.Count == 0)
            throw new Exception("No SAML Assertion found in the SOAP envelope.");

        var samlAssertion = assertion[0].OuterXml;

        var handler = new Saml2SecurityTokenHandler();
        var samlToken = handler.ReadSaml2Token(samlAssertion);

        //var samlToken = soapMessage;
        //if (samlToken != null)
        //{
        //    var assertion = sxmls.SerializeSoapMessageToXmlString(samlToken).Content;
        //}


        // Call the next delegate/middleware in the pipeline.
        await _next(httpContext);
    }
}


public static class PolicyEnforcementPointMiddlewareExtensions
{
    public static IApplicationBuilder UsePolicyEnforcementPointMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PolicyEnforcementPointMiddlware>();
    }
}