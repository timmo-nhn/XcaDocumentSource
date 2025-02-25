using XcaXds.Commons.Models.Soap;
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