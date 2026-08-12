using Microsoft.EntityFrameworkCore;
using XcaXds.Source.DatabaseRelations;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Implementations.RegistryRepository.SqLite;

public class SqliteRegistryDbContext : DbContext
{
    public DbSet<DbRegistryObject> RegistryObjects => Set<DbRegistryObject>();
    public DbSet<DbDocumentEntry> DocumentEntries => Set<DbDocumentEntry>();
    public DbSet<DbSubmissionSet> SubmissionSets => Set<DbSubmissionSet>();
    public DbSet<DbAssociation> Associations => Set<DbAssociation>();

    public SqliteRegistryDbContext(DbContextOptions<SqliteRegistryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        DbRelations.Setup(modelBuilder);
    }
}
