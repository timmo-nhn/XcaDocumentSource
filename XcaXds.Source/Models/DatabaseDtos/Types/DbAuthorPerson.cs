using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbAuthorPerson
{
    public string? Id { get; set; }
    public string? AssigningAuthority { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}