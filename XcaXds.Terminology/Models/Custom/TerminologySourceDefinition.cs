using XcaXds.Terminology.Mappers;

namespace XcaXds.Terminology.Models.Custom;

public class TerminologySourceDefinition
{
    public TerminologySourceDefinition(string name, TerminologySource<ICodeSystemMapper>[] terminologySources)
    {
        Name = name;
        TerminologySources = terminologySources;
    }

    public TerminologySourceDefinition() { }

    public string Name { get; set; } = string.Empty;

    public TerminologySource<ICodeSystemMapper>[] TerminologySources { get; set; }
}