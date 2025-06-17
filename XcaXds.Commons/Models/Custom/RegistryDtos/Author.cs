namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class Author
{
    public AuthorOrganization? Organization { get; set; }
    public AuthorOrganization? Department { get; set; }
    public AuthorPerson? Person { get; set; }
    public CodedValue? Role { get; set; }
    public CodedValue? Speciality { get; set; }
}