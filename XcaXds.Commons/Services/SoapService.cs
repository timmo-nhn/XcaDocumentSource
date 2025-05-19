using System.Text;
using XcaXds.Commons.Models.Soap;

namespace XcaXds.Commons.Services;

public class SoapService
{
    private readonly HttpClient _httpClient;

    public SoapService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> PostSoapRequestAsync<TResult>(SoapEnvelope request, string endpoint)
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var soapSerialization = sxmls.SerializeSoapMessageToXmlString(request);
        if (soapSerialization.IsSuccess)
        {
            var requestContent = new StringContent(soapSerialization.Content, Encoding.UTF8, Constants.MimeTypes.SoapXml);
            var response = await _httpClient.PostAsync(endpoint, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync();
                var soapResponse = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(content);
                var success = soapResponse.Header.Action is not Constants.Soap.Namespaces.AddressingSoapFault;
                return new SoapRequestResult<SoapEnvelope>() { Value = soapResponse, IsSuccess = success };
            }

            var errorContent = await response.Content.ReadAsStreamAsync();
            var soapington = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(errorContent);
            return new SoapRequestResult<SoapEnvelope>() { Value = soapington, IsSuccess = false };
        }

        var serializationFault = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(soapSerialization.Content);
        return new SoapRequestResult<SoapEnvelope>() { Value = serializationFault, IsSuccess = false };
    }


    //private static object GetRequestPayload<TInterface, TResult>(SoapEnvelope request)
    //{

    //    // Find the matching property in request.Body
    //    var bodyProperty = typeof(SoapBody).GetProperties()
    //        .FirstOrDefault(p => p.PropertyType == typeof(TInterface));

    //    if (bodyProperty == null)
    //    {
    //        throw new InvalidOperationException($"No matching property found in SoapBody for {typeof(TInterface).Name}");
    //    }

    //    // Get the actual value of the request body
    //    var requestPayload = bodyProperty.GetValue(request.Body);

    //    if (requestPayload == null)
    //    {
    //        throw new InvalidOperationException($"Request body property {bodyProperty.Name} is null.");
    //    }

    //    // Now invoke the SOAP method with the correct extracted request payload
    //    return requestPayload;
    //}
}