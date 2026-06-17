namespace XcaXds.Terminology.Models.Finnkode;

public class ValueSetCollection
{
    public string? id { get; set; }
    public string? name { get; set; }
    public List<Member>? members { get; set; }
}

public class Member
{
    public string? id { get; set; }
    public string? name { get; set; }
}
