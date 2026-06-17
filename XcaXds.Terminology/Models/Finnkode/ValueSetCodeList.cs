namespace XcaXds.Terminology.Models.Finnkode;

public class ValueSetCodeList
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateTime StatusLastChanged { get; set; }
    public Owner? Owner { get; set; }
    public Collection? Collection { get; set; }
    public Category? Category { get; set; }
    public List<Codevalue>? CodeValues { get; set; }
    public DateTime ValidFrom { get; set; }
    public bool Active { get; set; }
}

public class Owner
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class Collection
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class Category
{
    public string Id { get; set; } = default!; 
    public string? Name { get; set; }
}

public class Codevalue
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public string? SortKey { get; set; }
    public string? Description { get; set; }
    public DateTime ValidFrom { get; set; }
    public bool Active { get; set; }
}
