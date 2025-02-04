using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using XcaXds.Commons.Models.Soap;

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

    public object? GetImplementationOf<TInterface>()
    {
        return _serviceProvider.GetService(typeof(TInterface)); // Returns the registered implementation
    }


    public async Task<SoapRequestResult<TResult>> PostSoapRequestAsync<TInterface, TResult>(SoapEnvelope request, string endpoint)
    {
        var soapBodyType = typeof(SoapBody);
        var properties = soapBodyType.GetProperties();

        var matchingProperty = properties
            .FirstOrDefault(p => p.PropertyType.Name.Contains(typeof(TInterface).Name, StringComparison.OrdinalIgnoreCase));

        var body = request.Body;

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);
        var soapString = sxmls.SerializeSoapMessageToXmlString(body);


        using (var message = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, request.Header.Action,soapString))
        {
            var hmm = message.GetBody<SoapBody>();
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

    //public async Task<SoapRequestResult<TResponse>> PostSoapRequestAsync<TContract, TResponse>(object soapEnvelope, string endpointUrl)
    //    where TResponse : class
    //{
    //    if (endpointUrl == null)
    //    {
    //        throw new ArgumentNullException(nameof(endpointUrl));
    //    }

    //    if (soapEnvelope == null)
    //    {
    //        throw new ArgumentNullException(nameof(soapEnvelope));
    //    }

    //    var soapXmlSerializer = new SoapXmlSerializer();
    //    var soapString = soapXmlSerializer.SerializeSoapMessageToXmlString(soapEnvelope);

    //    var request = new HttpRequestMessage();
    //    request.RequestUri = new Uri(endpointUrl);
    //    request.Method = HttpMethod.Post;

    //    request.Content = new StringContent(soapString, Encoding.UTF8, "application/soap+xml");
    //    var response = await _httpClient.SendAsync(request);

    //    var responseContent = await response.Content.ReadAsStreamAsync();

    //    var responseXml = await soapXmlSerializer.DeserializeSoapMessageAsync<SoapEnvelope>(responseContent);

    //    var bodyType = typeof(SoapBody);

    //    var matchingProperty = bodyType.GetProperties()
    //        .FirstOrDefault(p => p.PropertyType == typeof(TResponse));

    //    if (matchingProperty != null)
    //    {
    //        var value = matchingProperty.GetValue(responseXml.Body);

    //        if (value != null)
    //        {
    //            return SoapRequestResult<TResponse>.Success(value as TResponse);
    //        }
    //    }

    //    var fault = responseXml.Body.Fault;
    //    var faultResponse = MapFaultToResponse<TResponse>(fault);

    //    return null;
    //}

    //private TResponse MapFaultToResponse<TResponse>(Fault fault) where TResponse : class
    //{
    //    // This is where you map the Fault object to a TResponse type
    //    // For instance, if TResponse is expected to be a registry response, return a dummy instance or a default one
    //    // You might want to map the fault message into the fields of TResponse, or even return a custom error response
    //    if (typeof(TResponse) == typeof(RegistryResponseType))
    //    {
    //        var registryResponse = new RegistryResponseType()
    //        {
    //            RegistryErrorList = new RegistryErrorList()
    //            {
    //                RegistryError = 
    //                [
    //                    new RegistryErrorType()
    //                    {
    //                        CodeContext = fault.Code?.Value,
    //                        Value = fault.Reason?.Text // Or any other meaningful mapping
    //                    }
    //                ]
    //            }
    //        };

    //        // For other response types, you can choose to return a default or error state.
    //        // This is just an example; adjust this mapping as per your actual response types
    //        return registryResponse as TResponse;
    //    }
    //    return default;
    //}
}