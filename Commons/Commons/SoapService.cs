using System.Text;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public class SoapService
{
    private readonly HttpClient _httpClient;

    public SoapService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SoapRequestResult<TResponse>> PostSoapRequestAsync<TContract, TResponse>(object soapEnvelope, string endpointUrl)
        where TResponse : class
    {
        if (endpointUrl == null)
        {
            throw new ArgumentNullException(nameof(endpointUrl));
        }

        if (soapEnvelope == null)
        {
            throw new ArgumentNullException(nameof(soapEnvelope));
        }

        var soapXmlSerializer = new SoapXmlSerializer();
        var soapString = soapXmlSerializer.SerializeSoapMessageToXmlString(soapEnvelope);

        var request = new HttpRequestMessage();
        request.RequestUri = new Uri(endpointUrl);
        request.Method = HttpMethod.Post;

        request.Content = new StringContent(soapString, Encoding.UTF8, "application/soap+xml");
        var response = await _httpClient.SendAsync(request);

        var responseContent = await response.Content.ReadAsStreamAsync();

        var responseXml = await soapXmlSerializer.DeserializeSoapMessageAsync<SoapEnvelope>(responseContent);

        var bodyType = typeof(SoapBody);

        var matchingProperty = bodyType.GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(TResponse));

        if (matchingProperty != null)
        {
            var value = matchingProperty.GetValue(responseXml.Body);

            if (value != null)
            {
                return SoapRequestResult<TResponse>.Success(value as TResponse);
            }
        }

        var fault = responseXml.Body.Fault;
        var faultResponse = MapFaultToResponse<TResponse>(fault);

        return null;
    }

    private TResponse MapFaultToResponse<TResponse>(Fault fault) where TResponse : class
    {
        // This is where you map the Fault object to a TResponse type
        // For instance, if TResponse is expected to be a registry response, return a dummy instance or a default one
        // You might want to map the fault message into the fields of TResponse, or even return a custom error response
        if (typeof(TResponse) == typeof(RegistryResponseType))
        {
            var registryResponse = new RegistryResponseType()
            {
                RegistryErrorList = new RegistryErrorList()
                {
                    RegistryError = 
                    [
                        new RegistryErrorType()
                        {
                            CodeContext = fault.Code?.Value,
                            Value = fault.Reason?.Text // Or any other meaningful mapping
                        }
                    ]
                }
            };

            // For other response types, you can choose to return a default or error state.
            // This is just an example; adjust this mapping as per your actual response types
            return registryResponse as TResponse;
        }
        return default;
    }
}