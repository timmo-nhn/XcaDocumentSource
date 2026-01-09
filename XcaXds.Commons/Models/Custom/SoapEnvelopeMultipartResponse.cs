using XcaXds.Commons.Models.Soap;

namespace XcaXds.Commons.Models.Custom;

public class SoapEnvelopeMultipartResponse
{
    public SoapEnvelope? SoapEnvelope { get; set; }

    public List<MultipartSection> MultiPartSections { get; set; } = new();
}

public class MultipartSection
{
    public string? ContentId { get; set; }
    public byte[]? Section { get; set; }
}