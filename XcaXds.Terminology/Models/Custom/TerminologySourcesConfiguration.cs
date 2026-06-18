namespace XcaXds.Terminology.Models.Custom;

public class TerminologySourcesConfiguration
{
    public List<TerminologySourceDefinitionConfiguration> Definitions { get; set; } = [];
}

public class TerminologySourceDefinitionConfiguration
{
    public string Name { get; set; } = string.Empty;
    public List<TerminologySourceConfiguration> Sources { get; set; } = [];
}

public class TerminologySourceConfiguration
{
    public string Type { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string Mapper { get; set; } = string.Empty;
    public TerminologyMapperOptions? MapperOptions { get; set; }
}

public class TerminologyMapperOptions
{
    public string? Separator { get; set; }
    public string? System { get; set; }
    public string? DisplayDiscriminator { get; set; }
}
