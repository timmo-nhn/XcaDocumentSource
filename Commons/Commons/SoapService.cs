using System.Text;
using XcaXds.Commons;

namespace XcaXds.Commons.Services;

public class SoapService
{
    private readonly HttpClient _httpClient;

    public SoapService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<SoapRequestResult<TResponse>> PostSoapRequestAsync<TContract, TResponse>(object soapEnvelope, string endpointUrl) where TResponse : class
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

        return new SoapRequestResult<TResponse> { IsSuccess = true, Value = null };
    }
}
