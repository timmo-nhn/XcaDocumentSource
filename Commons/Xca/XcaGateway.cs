using System.Net.Http.Headers;
using System.Text;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

namespace XcaXds.Commons.Xca;
public class XcaGateway
{
    private readonly SoapService _soapService;

    public XcaGateway(SoapService soapInvoker)
    {
        _soapService = soapInvoker;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> RegistryStoredQuery(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<SoapEnvelope>(request, endpoint);

        return response;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> RegisterDocumentSetb(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<SoapEnvelope>(request, endpoint);

        return response;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> RetrieveDocumentSet(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<SoapEnvelope>(request, endpoint);

        return response;
    }
}