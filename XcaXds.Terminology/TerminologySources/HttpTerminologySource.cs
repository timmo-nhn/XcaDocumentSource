using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.TerminologySources;

public class HttpTerminologySource : ITerminologySource
{
    private readonly IHttpClientFactory _clientFactory;

    public HttpTerminologySource(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<ComprehensiveCodeSystem?> FetchAsync(TerminologySource<ITerminologySource, ICodeSystemMapper> terminologySource)
    {
        var client = _clientFactory.CreateClient();
        // Client Authentication can be done here, for source endpoints requiring it...
        var clientRequest = new HttpRequestMessage(HttpMethod.Get, terminologySource.SourcePath);

        var response = await client.SendAsync(clientRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return terminologySource.MapperToUse.MapToComprehensiveCodeSystem(content);
    }
}