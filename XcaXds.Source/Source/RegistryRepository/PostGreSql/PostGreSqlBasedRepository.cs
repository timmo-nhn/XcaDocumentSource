using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared.Extensions;
using XcaXds.Source.Source.RegistryRepository.PostGreSql;

namespace XcaXds.Source.Source;

public class PostGreSqlBasedRepository : IRepository
{
    private readonly IDbContextFactory<PostGreSqlRepositoryDbContext> _contextFactory;

    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9\-_\.^]+$", RegexOptions.Compiled);
    private static readonly Regex SafeCharacters = new(@"[^a-zA-Z0-9\-_\.^]+", RegexOptions.Compiled);

    public PostGreSqlBasedRepository(IDbContextFactory<PostGreSqlRepositoryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();
        EnsureRepositoryTableExists(context);
    }

    public byte[]? Read(string documentUniqueId)
    {
        if (string.IsNullOrWhiteSpace(documentUniqueId))
        {
            return null;
        }

        var normalizedDocumentId = documentUniqueId.NoUrn();
        using var db = _contextFactory.CreateDbContext();

        return db.RepositoryDocuments
            .AsNoTracking()
            .Where(doc => doc.DocumentId == documentUniqueId || doc.NormalizedDocumentId == normalizedDocumentId)
            .Select(doc => doc.Data)
            .FirstOrDefault();
    }

    public OperationResponse Write(string documentId, byte[] data, string? patientId = null)
    {
        documentId = SafeCharacters.Replace(documentId, "");
        patientId = SafeCharacters.Replace(patientId ?? "", "");

        if (!IsValidIdentifier(documentId, out var invalidCharacters))
            return OperationResponse.Failure($"Invalid Document ID {documentId}, Invalid characters {invalidCharacters}");

        if (!IsValidIdentifier(patientId, out var invalidPatientIdCharacters))
            return OperationResponse.Failure($"Invalid Patient ID {patientId}, Invalid characters {invalidPatientIdCharacters}");

        var normalizedDocumentId = documentId.NoUrn();

        using var db = _contextFactory.CreateDbContext();
        var existing = db.RepositoryDocuments.FirstOrDefault(doc => doc.NormalizedDocumentId == normalizedDocumentId);

        if (existing == null)
        {
            db.RepositoryDocuments.Add(new()
            {
                DocumentId = documentId,
                NormalizedDocumentId = normalizedDocumentId,
                PatientId = patientId,
                Data = data
            });
        }
        else
        {
            existing.DocumentId = documentId;
            existing.PatientId = patientId;
            existing.Data = data;
        }

        db.SaveChanges();
        return OperationResponse.Success($"Document {documentId} stored in PostgreSQL repository");
    }

    public OperationResponse Delete(string? documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId)) return OperationResponse.Failure("No Document ID provided");

        documentId = SafeCharacters.Replace(documentId, "");
        var normalizedDocumentId = documentId.NoUrn();

        using var db = _contextFactory.CreateDbContext();
        var documentToDelete = db.RepositoryDocuments.FirstOrDefault(doc =>
            doc.DocumentId == documentId || doc.NormalizedDocumentId == normalizedDocumentId);

        if (documentToDelete == null)
        {
            return OperationResponse.Failure("Document not found");
        }

        db.RepositoryDocuments.Remove(documentToDelete);
        db.SaveChanges();

        return OperationResponse.Success($"Document {documentId} deleted successfully");
    }

    public bool SetNewOid(string repositoryUniqueId, out string? oldId)
    {
        oldId = null;
        return false;
    }

    private static bool IsValidIdentifier(string input, out string invalidCharacters)
    {
        invalidCharacters = string.Empty;
        if (string.IsNullOrEmpty(input)) return false;

        var matches = SafeFileNameRegex.Matches(input);
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                invalidCharacters += match.Value;
            }
        }

        return string.IsNullOrEmpty(invalidCharacters);
    }

    private static void EnsureRepositoryTableExists(PostGreSqlRepositoryDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "RepositoryDocuments" (
                "DocumentId" character varying(255) NOT NULL,
                "NormalizedDocumentId" character varying(255) NOT NULL,
                "PatientId" character varying(255),
                "Data" bytea NOT NULL,
                CONSTRAINT "PK_RepositoryDocuments" PRIMARY KEY ("DocumentId")
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_RepositoryDocuments_DocumentId"
            ON "RepositoryDocuments" ("DocumentId");
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_RepositoryDocuments_NormalizedDocumentId"
            ON "RepositoryDocuments" ("NormalizedDocumentId");
            """);
    }
}
