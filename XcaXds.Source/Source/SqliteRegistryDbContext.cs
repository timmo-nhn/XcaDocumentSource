using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source;

public class SqliteRegistryDbContext : DbContext
{
    public DbSet<DbRegistryObject> RegistryObjects => Set<DbRegistryObject>();
    public DbSet<DbDocumentEntry> DocumentEntries => Set<DbDocumentEntry>();
    public DbSet<DbSubmissionSet> SubmissionSets => Set<DbSubmissionSet>();
    public DbSet<DbAssociation> Associations => Set<DbAssociation>();

    private readonly string _dbPath;

    public SqliteRegistryDbContext(DbContextOptions<SqliteRegistryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbRegistryObject>().UseTpcMappingStrategy();

        modelBuilder.Entity<DbDocumentEntry>().ToTable("DocumentEntries");
        modelBuilder.Entity<DbSubmissionSet>().ToTable("SubmissionSets");
        modelBuilder.Entity<DbAssociation>().ToTable("Associations");

        var doc = modelBuilder.Entity<DbDocumentEntry>();

        doc.OwnsOne(d => d.ClassCode);
        doc.OwnsOne(d => d.TypeCode);
        doc.OwnsOne(d => d.FormatCode);
        doc.OwnsOne(d => d.PracticeSettingCode);
        doc.OwnsOne(d => d.HealthCareFacilityTypeCode);
        doc.OwnsOne(d => d.EventCodeList);
        doc.OwnsOne(d => d.LegalAuthenticator);
        doc.OwnsOne(d => d.SourcePatientInfo);

        doc.OwnsMany(d => d.Author, a =>
        {
            a.WithOwner().HasForeignKey("DocumentEntryId");
            a.ToTable("DocumentEntry_Authors");
            a.Property(x => x.Id).ValueGeneratedOnAdd();
        });
        doc.OwnsMany(d => d.ConfidentialityCode, a =>
        {
            a.WithOwner().HasForeignKey("DocumentEntryId");
            a.ToTable("DocumentEntry_ConfidentialityCodes");
            a.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        var sub = modelBuilder.Entity<DbSubmissionSet>();
        sub.OwnsMany(s => s.Author, a =>
        {
            a.WithOwner().HasForeignKey("SubmissionSetId");
            a.ToTable("SubmissionSet_Authors");
            a.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DbAssociation>();
    }
}
