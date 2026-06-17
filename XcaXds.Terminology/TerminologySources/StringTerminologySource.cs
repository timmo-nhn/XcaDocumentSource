using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.TerminologySources;

public class StringTerminologySource : ITerminologySource
{
    public async Task<ComprehensiveCodeSystem?> FetchAsync(TerminologySource<ITerminologySource, ICodeSystemMapper> terminologySource)
    {
        return terminologySource.MapperToUse.MapToComprehensiveCodeSystem(terminologySource.SourcePath);
    }
}