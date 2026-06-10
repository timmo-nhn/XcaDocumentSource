using XcaXds.Terminology.Mappers;

namespace XcaXds.Terminology.Models.Custom;

public class TerminologySource<TMapper> where TMapper : ICodeSystemMapper
{
    public TerminologySource(string sourcePath, TMapper mapperToUse) 
    {
        SourcePath = sourcePath;
        MapperToUse = mapperToUse;
    }

    public string SourcePath { get; set; } = string.Empty;
    // The implementation of ICodeSystemMapper to use
    public TMapper MapperToUse { get; set; }

}