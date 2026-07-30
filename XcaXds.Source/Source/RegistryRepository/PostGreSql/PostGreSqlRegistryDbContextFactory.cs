using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XcaXds.Source.Source.RegistryRepository.PostGreSql;

public class PostGreSqlRegistryDbContextFactory : IDesignTimeDbContextFactory<PostGreSqlRegistryDbContext>
{
    public PostGreSqlRegistryDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=xcadocumentsource;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PostGreSqlRegistryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PostGreSqlRegistryDbContext(options);
    }
}
