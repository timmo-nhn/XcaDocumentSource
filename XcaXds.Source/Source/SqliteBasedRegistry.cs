using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;
using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Source;

public class SqliteBasedRegistry : IRegistry
{
    private readonly ILogger<SqliteBasedRegistry> _logger;
    private readonly IDbContextFactory<SqliteRegistryDbContext> _contextFactory;

    private readonly string _connectionString;
    private readonly string _databaseFile;

    public SqliteBasedRegistry(
        ILogger<SqliteBasedRegistry> logger,
        IDbContextFactory<SqliteRegistryDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;

        _databaseFile = DatabasePathFinder.FindDatabasePath();

        _connectionString = $"Data Source=\"{_databaseFile}\"";

        _logger.LogDebug($"Database connection string: {_connectionString}");

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

		context.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
	}

    public string GetDatabaseFile()
    {
        return _databaseFile;
    }

    public IEnumerable<RegistryObjectDto> ReadRegistry()
    {
        using var db = _contextFactory.CreateDbContext();

        foreach (var entity in db.RegistryObjects.AsNoTracking())
        {
            var entityDto = DatabaseMapper.MapFromDatabaseEntityToDto(entity);

            if (entityDto != null)
            {
                yield return entityDto;
            }
        }
    }

    public bool UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        using var db = _contextFactory.CreateDbContext();

        // Map once
        var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);

        // Ensure IDs exist (your logic)
        foreach (var e in dbEntities)
        {
            if (string.IsNullOrWhiteSpace(e.Id))
                e.Id = Guid.NewGuid().ToString();
        }

        // Split
        var documentEntries = dbEntities.OfType<DbDocumentEntry>().ToList();
        var submissionSets = dbEntities.OfType<DbSubmissionSet>().ToList();
        var associations = dbEntities.OfType<DbAssociation>().ToList();


        // Perf knobs
        db.ChangeTracker.AutoDetectChangesEnabled = false;
        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // mostly for queries, harmless here

        // SQLite-specific: reduces fsync overhead a lot for bulk-ish writes.
        // Safe within a transaction, but still a trade-off—remove if you can't allow it.
        //db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
		db.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
		db.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");

        using var transaction = db.Database.BeginTransaction();

        const int batchSize = 2_000; // tune: 500..10_000 depending on payload size

        InsertInBatches(db, documentEntries, batchSize);
        InsertInBatches(db, submissionSets, batchSize);
        InsertInBatches(db, associations, batchSize);

        var asscount = db.Associations.Count();
        var robjcount = db.DocumentEntries.Count();
        var subscount = db.SubmissionSets.Count();

        transaction.Commit();
        return true;
    }

    private static void InsertInBatches<T>(DbContext db, List<T> items, int batchSize) where T : class
    {
        if (items.Count == 0) return;

        for (int i = 0; i < items.Count; i += batchSize)
        {
            var batch = items.Skip(i).Take(batchSize).ToList();

            // Add without calling DetectChanges repeatedly
            db.Set<T>().AddRange(batch);

            // Persist this chunk
            db.SaveChanges();

            // IMPORTANT: prevent EF from accumulating tracked entities and slowing down / bloating memory
            db.ChangeTracker.Clear();
        }
    }

    public bool InsertOrUpdateRegistry(List<RegistryObjectDto> dtos)
    {
        using var db = _contextFactory.CreateDbContext();
        var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);

        foreach (var e in dbEntities)
            if (string.IsNullOrWhiteSpace(e.Id))
                e.Id = Guid.NewGuid().ToString();

        var documentEntries = dbEntities.OfType<DbDocumentEntry>().ToList();
        var submissionSets = dbEntities.OfType<DbSubmissionSet>().ToList();
        var associations = dbEntities.OfType<DbAssociation>().ToList();		

		db.ChangeTracker.AutoDetectChangesEnabled = false;
        
        //using var transaction = db.Database.BeginTransaction();

        const int idBatchSize = 300;  // keep modest for SQLite parameter limits
        const int insertBatchSize = 500;  // tune based on entity size

        DeleteThenInsertBatched(db, db.DocumentEntries, documentEntries, idBatchSize, insertBatchSize);
        DeleteThenInsertBatched(db, db.SubmissionSets, submissionSets, idBatchSize, insertBatchSize);
        DeleteThenInsertBatched(db, db.Associations, associations, idBatchSize, insertBatchSize);

        //transaction.Commit();
        return true;
    }
	
	private static void DeleteThenInsertBatched<TEntity>(
        DbContext db,
        DbSet<TEntity> set,
        List<TEntity> incoming,
        int idBatchSize,
        int insertBatchSize)
        where TEntity : DbRegistryObject
    {
        if (incoming.Count == 0) return;

        // Ensure distinct IDs to avoid duplicates
        incoming = incoming
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .GroupBy(x => x.Id!)
            .Select(g => g.Last())
            .ToList();

        var ids = incoming.Select(x => x.Id!).ToList();
        if (ids.Count == 0) return;

        // 1) Delete existing in ID batches
        foreach (var idBatch in Batch(ids, idBatchSize))
        {
            // For owned collections:
            // If your DB schema has cascade delete for owned types (typical), you do NOT need Include().
            // If you don't have cascade, you'll need Include() like you had, or delete owned rows separately.
            var existing = set
                .Where(x => x.Id != null && idBatch.Contains(x.Id))
                .ToList();

            if (existing.Count > 0)
            {
				Console.WriteLine("Trying to delete existing, count = {0}", existing.Count);

				set.RemoveRange(existing);
                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
        }        
        
        // 2) Insert in batches
        for (int i = 0; i < incoming.Count; i += insertBatchSize)
        {
            var batch = incoming.Skip(i).Take(insertBatchSize).ToList();
            set.AddRange(batch);

            var sql = set.ToQueryString(); // for debugging - shows the SQL EF will execute for this batch

            Console.WriteLine(sql);

			db.SaveChanges();
            db.ChangeTracker.Clear();
        }
    }

    private static IEnumerable<List<T>> Batch<T>(List<T> items, int size)
    {
        for (int i = 0; i < items.Count; i += size)
            yield return items.GetRange(i, Math.Min(size, items.Count - i));
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        using var db = _contextFactory.CreateDbContext();
        var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);
        db.RegistryObjects.RemoveRange(db.RegistryObjects);
        db.RegistryObjects.AddRange(dbEntities);
        db.SaveChanges();
        return true;
    }

    public bool DeleteRegistryItem(string id)
    {
        using var db = _contextFactory.CreateDbContext();
        var registryObjectToDelete = db.RegistryObjects.FirstOrDefault(ro => ro.Id == id);

        if (registryObjectToDelete == null) return false;

        db.RegistryObjects.Remove(registryObjectToDelete);
        db.SaveChanges();
        return true;
    }
}