namespace XcaXds.WebService.InputFormatters;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;


/// <summary>
/// For validation errors that occur in between the inputformatter and controller part of the middleware pipeline
/// </summary>
public static class SoapFaultErrorResponseFactory
{
    public static IActionResult CreateErrorResponse(ActionContext context)
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var soapFault = SoapExtensions.CreateSoapFault
        (
            faultCode: "XML model validation error",
            subCode: "SubCode",
            faultReason: string.Join("; ", context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
        );

        var soapFaultString = sxmls.SerializeSoapMessageToXmlString(soapFault.Value).Content;


        return new ContentResult
        {
            Content = soapFaultString,
            ContentType = "application/xml",
            StatusCode = 400
        };
    }
}
