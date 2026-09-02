using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XcaXds.Source.Implementations.Statistics.PostGreSql;

public class StatisticsDbContextFactory : IDesignTimeDbContextFactory<StatisticsDbContext>
{
    public StatisticsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=xcadocumentsource;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<StatisticsDbContext>()
            .UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Statistics"))
            .Options;

        return new StatisticsDbContext(options);
    }
}
