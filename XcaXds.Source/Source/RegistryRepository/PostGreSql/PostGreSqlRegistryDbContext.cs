using Microsoft.EntityFrameworkCore;
using XcaXds.Source.DatabaseRelations;
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
        DbRelations.Setup(modelBuilder);

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
