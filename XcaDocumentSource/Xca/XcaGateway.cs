
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Models.Soap.Actions;
using XcaDocumentSource.Models.Soap.Xds;
using XcaDocumentSource.Services;

namespace XcaDocumentSource.Xca;
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
}