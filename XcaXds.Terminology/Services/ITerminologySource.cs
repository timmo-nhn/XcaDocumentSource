using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public interface ITerminologySource
{
    Task<ComprehensiveCodeSystem> FetchAsync(string sourceIdentifier);
}