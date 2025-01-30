
using XcaGatewayService.Models.Soap;
using XcaGatewayService.Services;

namespace XcaGatewayService.Xca;
public class XcaGateway
{
    private readonly SoapService _soapService;


    public XcaGateway(SoapService soapInvoker)
    {
        _soapService = soapInvoker;
    }

    public async Task<SoapRequestResult<AdhocQueryResponse>> RegistryStoredQuery(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<IRegistryStoredQuery, AdhocQueryResponse>(request, endpoint);

        return response;
    }

    public async Task<SoapRequestResult<RegisterDocumentSetbResponse>> RegisterDocumentSet(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<IRegisterDocumentSetb, RegisterDocumentSetbResponse>(request, endpoint);

        return response;
    }
}