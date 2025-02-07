using System.Data.SqlTypes;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public class SoapService
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;

    public SoapService(HttpClient httpClient, IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> PostSoapRequestAsync<TResult>(SoapEnvelope request, string endpoint)
    {
        // PostSoapRequestAsync<IRegisterDocumentSetb, RegistryResponseType>
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var soapString = sxmls.SerializeSoapMessageToXmlString(request);

        var requestContent = new StringContent(soapString, Encoding.UTF8, Constants.MimeTypes.SoapXml);

        var response = await _httpClient.PostAsync(endpoint, requestContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStreamAsync();

            var soapResponse = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(content);
            var success = soapResponse.Header.Action is not Constants.Soap.Namespaces.AddressingSoapFault;
            return new SoapRequestResult<SoapEnvelope>() { Value = soapResponse, IsSuccess = success };
        }

        return null;
    }


    private static object GetRequestPayload<TInterface, TResult>(SoapEnvelope request)
    {

        // Find the matching property in request.Body
        var bodyProperty = typeof(SoapBody).GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(TInterface));

        if (bodyProperty == null)
        {
            throw new InvalidOperationException($"No matching property found in SoapBody for {typeof(TInterface).Name}");
        }

        // Get the actual value of the request body
        var requestPayload = bodyProperty.GetValue(request.Body);

        if (requestPayload == null)
        {
            throw new InvalidOperationException($"Request body property {bodyProperty.Name} is null.");
        }

        // Now invoke the SOAP method with the correct extracted request payload
        return requestPayload;
    }
}