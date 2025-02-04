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

    public async Task<SoapRequestResult<RegistryStoredQueryResponse>> RegistryStoredQuery(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<IRegistryStoredQuery, RegistryStoredQueryResponse>(request, endpoint);

        return response;
    }

    public async Task<SoapRequestResult<RegisterDocumentSetbResponse>> RegisterDocumentSetb(SoapEnvelope request, string endpoint)
    {
        var response = await _soapService.PostSoapRequestAsync<IRegisterDocumentSetb, RegisterDocumentSetbResponse>(request, endpoint);

        return response;
    }
}