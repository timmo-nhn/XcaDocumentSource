using XcaXds.Terminology.Interfaces;

namespace XcaXds.Terminology.Models.Custom;

public class TerminologySourceDefinition
{
    public TerminologySourceDefinition(string name, TerminologySource<ITerminologySource, ICodeSystemMapper>[] terminologySources)
    {
        Name = name;
        TerminologySources = terminologySources;
    }

    public TerminologySourceDefinition() { }

    public string Name { get; set; } = string.Empty;

    public TerminologySource<ITerminologySource, ICodeSystemMapper>[] TerminologySources { get; set; } = [];
}