using Microsoft.EntityFrameworkCore;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Implementations.Statistics.PostGreSql;

public class StatisticsDbContext : DbContext
{
    public DbSet<DbUserAccessEntry> UserAccessEntries => Set<DbUserAccessEntry>();

    public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbUserAccessEntry>(entity =>
        {
            entity.HasIndex(e => e.AccessTime);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.SessionId);
            entity.Property(e => e.AccessTime).HasColumnType("timestamp with time zone");
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeKinds();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeKinds();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimeKinds()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                continue;

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(DateTime) &&
                    property.Metadata.ClrType != typeof(DateTime?))
                    continue;

                if (property.CurrentValue is DateTime dt)
                {
                    property.CurrentValue = dt.Kind switch
                    {
                        DateTimeKind.Utc => dt,
                        DateTimeKind.Local => dt.ToUniversalTime(),
                        _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                    };
                }
            }
        }
    }
}
