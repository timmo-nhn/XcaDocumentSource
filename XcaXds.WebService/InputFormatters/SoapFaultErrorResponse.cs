namespace XcaXds.WebService.InputFormatters;

using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Serializers;


/// <summary>
/// For validation errors that occur in between the inputformatter and controller part of the middleware pipeline<para/>
/// Ie. this functions is triggered when the user sends a malformed payload
/// </summary>
public static class ErrorResponseFactory
{
    public static IActionResult CreateErrorResponse(ActionContext context)
    {
        var contentType = context.HttpContext.Request.ContentType;

        switch (contentType?.Split(";").FirstOrDefault())
        {
            case Constants.MimeTypes.MultipartRelated:
            case Constants.MimeTypes.SoapXml:
                return CreateSoapErrorResponse(context);

            default:
                return CreateJsonErrorResponse(context);
        }
    }

    private static BadRequestObjectResult CreateJsonErrorResponse(ActionContext context)
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        };

        return new BadRequestObjectResult(problemDetails);
    }

    private static ContentResult CreateSoapErrorResponse(ActionContext context)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

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
