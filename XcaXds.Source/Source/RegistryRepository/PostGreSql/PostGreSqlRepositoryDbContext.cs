using Microsoft.EntityFrameworkCore;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source.RegistryRepository.PostGreSql;

public class PostGreSqlRepositoryDbContext : DbContext
{
    public DbSet<DbRepositoryDocument> RepositoryDocuments => Set<DbRepositoryDocument>();

    public PostGreSqlRepositoryDbContext(DbContextOptions<PostGreSqlRepositoryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbRepositoryDocument>().ToTable("RepositoryDocuments");
    }
}
