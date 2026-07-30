using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XcaXds.Source.Source.RegistryRepository.SqLite;

public class SqliteRegistryDbContextFactory : IDesignTimeDbContextFactory<SqliteRegistryDbContext>
{
    public SqliteRegistryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteRegistryDbContext>()
            .UseSqlite("Data Source=Registry.db")
            .Options;

        return new SqliteRegistryDbContext(options);
    }
}
