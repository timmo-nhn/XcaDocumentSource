using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbLegalAuthenticator
{
    public string? Id { get; set; }
    public string? IdSystem { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}