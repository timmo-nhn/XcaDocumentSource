using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Interfaces;

public interface ITerminologySource
{
    Task<ComprehensiveCodeSystem?> FetchAsync(TerminologySource<ITerminologySource, ICodeSystemMapper> terminologySource);
}