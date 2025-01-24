using Microsoft.AspNetCore.Identity.Data;
using System.Text;
using XcaDocumentSource.Models.Soap;

namespace XcaDocumentSource.Services
{
    public class SoapRequestResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public Fault? FaultResult { get; set; }

        public static SoapRequestResult<T> Success(T result)
        {
            var soapResult = new SoapRequestResult<T>
            {
                IsSuccess = true,
                Value = result
            };

            return soapResult;
        }

        public static SoapRequestResult<SoapEnvelope> Fault(Fault fault)
        {
            var soapFaultEnvelope = new SoapEnvelope()
            {
                Header = new()
                {
                    Action = Constants.Soap.Namespaces.AddressingSoapFault
                },
                Body = new() { Fault = fault }
            };

            var soapResult = new SoapRequestResult<SoapEnvelope>
            {
                IsSuccess = false,
                Value = soapFaultEnvelope,
                FaultResult = fault
            };

            return soapResult;
        }
    }

    public class SoapService
    {
        private readonly HttpClient _httpClient;

        public SoapService(HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }
        public async Task<SoapRequestResult<TResponse>>PostSoapRequestAsync<TContract, TResponse>(object soapEnvelope,string endpointUrl) where TResponse : class
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

            request.Content = new StringContent(soapString,Encoding.UTF8,"application/soap+xml");
            var response = await _httpClient.SendAsync(request);

            return new SoapRequestResult<TResponse> {IsSuccess = true,Value = null};
        }
    }
}
