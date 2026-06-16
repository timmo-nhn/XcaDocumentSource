using Microsoft.EntityFrameworkCore;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source;

public class SqliteRegistryDbContext : DbContext
{
    public DbSet<DbRegistryObject> RegistryObjects => Set<DbRegistryObject>();
    public DbSet<DbDocumentEntry> DocumentEntries => Set<DbDocumentEntry>();
    public DbSet<DbSubmissionSet> SubmissionSets => Set<DbSubmissionSet>();
    public DbSet<DbAssociation> Associations => Set<DbAssociation>();

    public SqliteRegistryDbContext(DbContextOptions<SqliteRegistryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var robj = modelBuilder.Entity<DbRegistryObject>()
            .UseTphMappingStrategy();

        modelBuilder.Entity<DbRegistryObject>().ToTable("RegistryObjects");

        var doc = modelBuilder.Entity<DbDocumentEntry>();
        doc.HasIndex(d => d.DE_UniqueId).IsUnique();
        doc.HasIndex(d => new { d.DE_SourcePatientInfoPatientId, d.DE_SourcePatientInfoPatientSystem });

        doc.ComplexProperty(d => d.DE_ClassCode).HasDiscriminator();
        doc.ComplexProperty(d => d.DE_TypeCode).HasDiscriminator();
        doc.ComplexProperty(d => d.DE_FormatCode).HasDiscriminator();
        doc.ComplexProperty(d => d.DE_PracticeSettingCode).HasDiscriminator();
        doc.ComplexProperty(d => d.DE_HealthCareFacilityTypeCode).HasDiscriminator();
        doc.ComplexProperty(d => d.DE_EventCodeList).HasDiscriminator();
        doc.ComplexProperty(d => d.DE_LegalAuthenticator).HasDiscriminator();

        doc.OwnsMany(d => d.DE_Author, a =>
        {
            a.WithOwner().HasForeignKey("DocumentEntryId");
            a.ToTable("DocumentEntry_Authors");
            a.Property(x => x.Id).ValueGeneratedOnAdd();
        });
        doc.OwnsMany(d => d.DE_ConfidentialityCode, a =>
        {
            a.WithOwner().HasForeignKey("DocumentEntryId");
            a.ToTable("DocumentEntry_ConfidentialityCodes");
            a.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        var sub = modelBuilder.Entity<DbSubmissionSet>();
        sub.OwnsMany(s => s.SS_Author, a =>
        {
            a.WithOwner().HasForeignKey("SubmissionSetId");
            a.ToTable("SubmissionSet_Authors");
            a.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DbAssociation>()
            .HasOne<DbRegistryObject>()
            .WithMany()
            .HasForeignKey(a => a.AS_SourceObjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DbAssociation>()
            .HasOne<DbRegistryObject>()
            .WithMany()
            .HasForeignKey(a => a.AS_TargetObjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
