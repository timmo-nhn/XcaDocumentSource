using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace XcaXds.Source.Models.DatabaseDtos;

[Index(nameof(DocumentId), IsUnique = true)]
[Index(nameof(NormalizedDocumentId), IsUnique = true)]
public class DbRepositoryDocument
{
    [Key]
    [StringLength(255)]
    public required string DocumentId { get; set; }

    [StringLength(255)]
    public required string NormalizedDocumentId { get; set; }

    [StringLength(255)]
    public string? PatientId { get; set; }

    public required byte[] Data { get; set; }
}
