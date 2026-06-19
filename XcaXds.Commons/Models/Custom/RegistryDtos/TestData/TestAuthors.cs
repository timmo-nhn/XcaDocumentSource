using XcaXds.Shared.Models.Custom;

namespace XcaXds.Commons.Models.Custom.RegistryDtos.TestData;

public class TestAuthors
{
    public List<AuthorOrganization> Organizations { get; set; } = new();
    public List<AuthorOrganization> Departments { get; set; } = new();
    public List<AuthorPerson> Persons { get; set; } = new();
    public List<CodedValue> Roles { get; set; } = new();
    public List<CodedValue> Specialities { get; set; } = new();
}