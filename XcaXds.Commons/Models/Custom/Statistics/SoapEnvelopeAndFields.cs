using Microsoft.AspNetCore.Http;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;

namespace XcaXds.Commons.Models.Custom.Statistics;

public class SoapEnvelopeAndFields
{
    public required DateTime AccessTime { get; init; } = DateTime.UtcNow;
    public required SoapEnvelope SoapEnvelope { get; init; }
    public required long ElapsedMilliseconds { get; init; }
    public required string Path { get; init; }
    public required string Method { get; init; }
    public required int StatusCode { get; init; }
    public CodedValue[]? ConfidentialityCodes { get; set; }
}
