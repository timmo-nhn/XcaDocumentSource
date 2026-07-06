using Microsoft.EntityFrameworkCore;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source.RegistryRepository.PostGreSql;

public class PostGreSqlRegistryDbContext : DbContext
{
    public DbSet<DbRegistryObject> RegistryObjects => Set<DbRegistryObject>();
    public DbSet<DbDocumentEntry> DocumentEntries => Set<DbDocumentEntry>();
    public DbSet<DbSubmissionSet> SubmissionSets => Set<DbSubmissionSet>();
    public DbSet<DbAssociation> Associations => Set<DbAssociation>();

    public PostGreSqlRegistryDbContext(DbContextOptions<PostGreSqlRegistryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbRegistryObject>().UseTphMappingStrategy();
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

        ConfigureDateTimeColumnsAsTimestampWithTimeZone(modelBuilder);
    }

    private static void ConfigureDateTimeColumnsAsTimestampWithTimeZone(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeKindsForPostgreSql();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeKindsForPostgreSql();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimeKindsForPostgreSql()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(DateTime) &&
                    property.Metadata.ClrType != typeof(DateTime?))
                {
                    continue;
                }

                if (property.CurrentValue is DateTime dateTimeValue)
                {
                    property.CurrentValue = dateTimeValue.Kind switch
                    {
                        DateTimeKind.Utc => dateTimeValue,
                        DateTimeKind.Local => dateTimeValue.ToUniversalTime(),
                        _ => DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Utc)
                    };
                }
            }
        }
    }
}
