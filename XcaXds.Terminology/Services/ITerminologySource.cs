using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public interface ITerminologySource
{
    Task<ComprehensiveCodeSystem?> FetchAsync(TerminologySource<ICodeSystemMapper> terminologySource);
}