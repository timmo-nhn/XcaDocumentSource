using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;

namespace XcaXds.Commons.Models.Custom.Statistics;

/// <summary>
/// Container class for properties used to generate statistics
/// </summary>
public class StatisticsRequestAndFields
{
    public DateTime AccessTime { get; init; } = DateTime.UtcNow;
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? JwtToken { get; set; }
    public long ElapsedMilliseconds { get; init; }
    public string? Path { get; init; }
    public string? Method { get; init; }
    public int StatusCode { get; init; }
    public RequestAndFieldRequestType RequestType { get; set; }
    public string? SessionId { get; set; }
    public DocumentEntryDto[]? RelatedDocumentEntries { get; set; }
}
