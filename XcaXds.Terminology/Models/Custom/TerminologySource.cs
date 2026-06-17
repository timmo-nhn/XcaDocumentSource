using XcaXds.Terminology.Interfaces;
using XcaXds.Terminology.Services;

namespace XcaXds.Terminology.Models.Custom;

public class TerminologySource<TSource, TMapper>
    where TSource : ITerminologySource
    where TMapper : ICodeSystemMapper
{
    /// <summary>
    /// Defines a source for a code system, including the path to the source and the mapper to use for converting the raw content to a ComprehensiveCodeSystem.
    /// </summary>
    /// <param name="source">The source of the code system.</param>
    /// <param name="sourcePath">The path to the source of the code system (Will be resolved and used by the appropriate implementation according to <see cref="TerminologySourceFactory">)</param>
    /// <param name="mapperToUse">The mapper to use for converting the raw content to a ComprehensiveCodeSystem.</param>
    public TerminologySource(TSource source, string sourcePath, TMapper mapperToUse)
    {
        Source = source;
        SourcePath = sourcePath;
        MapperToUse = mapperToUse;
    }

    public TSource Source { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    // The implementation of ICodeSystemMapper to use
    public TMapper MapperToUse { get; set; }

}