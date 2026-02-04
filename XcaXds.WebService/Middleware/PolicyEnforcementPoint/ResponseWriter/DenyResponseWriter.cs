//using Hl7.Fhir.Model;
//using Hl7.Fhir.Serialization;
//using System.Net;
//using System.Text.Json;
//using XcaXds.Commons.Commons;
//using XcaXds.Commons.Extensions;
//using XcaXds.Commons.Models.Custom;
//using XcaXds.Commons.Models.Custom.RestfulRegistry;
//using XcaXds.Commons.Models.Soap;
//using XcaXds.Commons.Models.Soap.XdsTypes;
//using XcaXds.Commons.Serializers;
//using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;
//using Task = System.Threading.Tasks.Task;

//namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.ResponseWriters;

//public static class DenyResponseWriter
//{
//    public static async Task WriteBadRequestAsync(HttpContext httpContext, ApplicationConfig appConfig, string errorMessage)
//    {
//        var contentType = httpContext.Request.ContentType?.Split(";").FirstOrDefault();

//        if (contentType == Constants.MimeTypes.XopXml ||
//            contentType == Constants.MimeTypes.MultipartRelated ||
//            contentType == Constants.MimeTypes.SoapXml ||
//            contentType == Constants.MimeTypes.Xml)
//        {
//            var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

//            var soapResponseEnvelope = new SoapEnvelope();
//            soapResponseEnvelope = SoapExtensions.CreateSoapFault("Sender", null, errorMessage).Value;

//            if (contentType == Constants.MimeTypes.XopXml ||
//                contentType == Constants.MimeTypes.MultipartRelated)
//            {
//                var soapMultipart = MultipartExtensions.ConvertRetrieveDocumentSetResponseToMultipartResponse(soapResponseEnvelope, out var boundary);

//                var contentId = string.Empty;

//                if (soapMultipart.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var contentIdValues) ?? false)
//                {
//                    contentId = contentIdValues.First().TrimStart('<').TrimEnd('>');
//                }

//                httpContext.Response.ContentType = $"{Constants.MimeTypes.MultipartRelated}; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"";

//                var soapByteArray = await MultipartExtensions.SerializeMultipartAsync(soapMultipart);

//                await httpContext.Response.Body.WriteAsync(soapByteArray);
//            }
//            else
//            {
//                httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

//                var soapResponseString = sxmls.SerializeSoapMessageToXmlString(soapResponseEnvelope).Content;

//                await httpContext.Response.WriteAsync(soapResponseString ?? string.Empty);
//            }
//        }

//        else if (contentType == Constants.MimeTypes.FhirJson)
//        {
//            var fhirJsonSerializer = new FhirJsonSerializer();

//            httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
//            httpContext.Response.ContentType = Constants.MimeTypes.FhirJson;

//            var fhirJsonResponse = OperationOutcome.ForMessage("Access denied", OperationOutcome.IssueType.Forbidden, OperationOutcome.IssueSeverity.Error);
//            await httpContext.Response.WriteAsync(fhirJsonSerializer.SerializeToString(fhirJsonResponse, true));

//        }
//    }

//    public static async Task WriteDenyAsync(HttpContext httpContext, PolicyInputResult inputResult, ApplicationConfig appConfig, string errorMessage)
//    {

//        var contentType = httpContext.Request.ContentType?.Split(";").FirstOrDefault();

//        if (contentType == Constants.MimeTypes.XopXml ||
//            contentType == Constants.MimeTypes.MultipartRelated ||
//            contentType == Constants.MimeTypes.SoapXml ||
//            contentType == Constants.MimeTypes.Xml)
//        {
            
            
//            var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
//            var soapEnvelopeRequest = sxmls.DeserializeXmlString<SoapEnvelope>(requestBody);

//            var soapEnvelopeResponse = new SoapEnvelope()
//            {
//                Header = new()
//                {
//                    Action = soapEnvelopeRequest.GetCorrespondingResponseAction(),
//                    MessageId = Guid.NewGuid().ToString(),
//                    RelatesTo = soapEnvelopeRequest.Header.MessageId,
//                },
//                Body = new()
//            };

//            var registryResponse = new RegistryResponseType();
//            registryResponse.AddError(XdsErrorCodes.XDSRegistryError, errorMessage, appConfig.HomeCommunityId);

//            SoapExtensions.PutRegistryResponseInTheCorrectPlaceAccordingToSoapAction(soapEnvelopeResponse, registryResponse);

//            if (contentType == Constants.MimeTypes.XopXml ||
//                contentType == Constants.MimeTypes.MultipartRelated)
//            {
//                var requestBody = await MultipartExtensions.ReadMultipartContentFromRequest(httpContext.Request);
//                var soapMultipart = MultipartExtensions.ConvertRetrieveDocumentSetResponseToMultipartResponse(soapEnvelopeResponse, out var boundary);

//                var contentId = string.Empty;

//                if (soapMultipart.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var contentIdValues) ?? false)
//                {
//                    contentId = contentIdValues.First().TrimStart('<').TrimEnd('>');
//                }

//                httpContext.Response.ContentType = $"{Constants.MimeTypes.MultipartRelated}; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"";

//                var soapByteArray = await MultipartExtensions.SerializeMultipartAsync(soapMultipart);

//                await httpContext.Response.Body.WriteAsync(soapByteArray);
//            }
//            else
//            {
//                httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

//                var soapResponseString = sxmls.SerializeSoapMessageToXmlString(soapEnvelopeResponse).Content;

//                await httpContext.Response.WriteAsync(soapResponseString ?? string.Empty);
//            }
//        }
//        else if (contentType == Constants.MimeTypes.FhirJson)
//        {
//            var fhirJsonSerializer = new FhirJsonSerializer();

//            httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
//            httpContext.Response.ContentType = Constants.MimeTypes.FhirJson;

//            var fhirJsonResponse = OperationOutcome.ForMessage("Access denied", OperationOutcome.IssueType.Forbidden, OperationOutcome.IssueSeverity.Error);
//            await httpContext.Response.WriteAsync(fhirJsonSerializer.SerializeToString(fhirJsonResponse, true));
//        }
//        else // if (contentType == Constants.MimeTypes.Json)
//        {
//            var restfulApiResponse = new RestfulApiResponse(false, "Access denied");
//            httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
//            httpContext.Response.ContentType = Constants.MimeTypes.Json;

//            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(restfulApiResponse, Constants.JsonDefaultOptions.DefaultSettings));
//        }
//    }
//}
