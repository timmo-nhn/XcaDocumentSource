using XcaXds.Terminology.Interfaces;

namespace XcaXds.Terminology.Models.Custom;

public class TerminologySource<ITerminologySource, ICodeSystemMapper>
{
    /// <summary>
    /// Defines a source for a code system, including the path to the source and the mapper to use for converting the raw content to a ComprehensiveCodeSystem.
    /// </summary>
    /// <param name="source">The source of the code system.</param>
    /// <param name="sourcePath">The path to the source of the code system (Will be resolved and used by the appropriate implementation according to <see cref="TerminologySourceFactory">)</param>
    /// <param name="mapperToUse">The mapper to use for converting the raw content to a ComprehensiveCodeSystem.</param>
    public TerminologySource(ITerminologySource source, string sourcePath, ICodeSystemMapper mapperToUse)
    {
        Source = source;
        SourcePath = sourcePath;
        MapperToUse = mapperToUse;
    }

    public TerminologySource(ITerminologySource source, string[] values, ICodeSystemMapper mapperToUse)
    {
        Source = source;
        Values = values;
        MapperToUse = mapperToUse;
    }

    public ITerminologySource Source { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    // Hardcoded codesystem values, if applicable
    public string[]? Values { get; set; }
    // The implementation of ICodeSystemMapper to use
    public ICodeSystemMapper MapperToUse { get; set; }

}