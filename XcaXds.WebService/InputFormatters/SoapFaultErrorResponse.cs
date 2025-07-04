﻿namespace XcaXds.WebService.InputFormatters;

using System;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Services;


/// <summary>
/// For validation errors that occur in between the inputformatter and controller part of the middleware pipeline
/// IE.: This functions is triggered when the user sends a malformed payload
/// </summary>
public static class ErrorResponseFactory
{
    public static IActionResult CreateErrorResponse(ActionContext context)
    {
        var contentType = context.HttpContext.Request.ContentType;

        switch (contentType)
        {

            case "application/soap+xml":
                return CreateSoapErrorResponse(context);

            default:
                return CreateJsonErrorResponse(context);
        }
    }

    private static IActionResult CreateJsonErrorResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors?.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value?.Errors?.Select(err => err.ErrorMessage)
            });

        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        };

        return new BadRequestObjectResult(problemDetails);

    }

    private static IActionResult CreateSoapErrorResponse(ActionContext context)
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
