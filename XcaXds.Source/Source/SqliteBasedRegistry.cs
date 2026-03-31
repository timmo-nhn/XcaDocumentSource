using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source;

public class SqliteBasedRegistry : IRegistry
{
    private readonly ILogger<SqliteBasedRegistry> _logger;
    private readonly IDbContextFactory<SqliteRegistryDbContext> _contextFactory;

    private readonly string _connectionString;
    private readonly string _databaseFile;

    public SqliteBasedRegistry(ILogger<SqliteBasedRegistry> logger, IDbContextFactory<SqliteRegistryDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;

        _databaseFile = DatabasePathFinder.FindDatabasePath();

        _connectionString = $"Data Source=\"{_databaseFile}\"";

        _logger.LogDebug($"Database connection string: {_connectionString}");

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        //context.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
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

    public IEnumerable<RegistryObjectDto> GetRegistryItemsForPatient(PatientId patientIdentifier)
    {
        using var db = _contextFactory.CreateDbContext();
        var entities = db.DocumentEntries
            .AsNoTracking()
            .Where(de =>
                de.DE_SourcePatientInfoPatientId == patientIdentifier.Id &&
                de.DE_SourcePatientInfoPatientSystem == patientIdentifier.System);

        foreach (var entity in entities)
        {
            var entityDto = DatabaseMapper.MapFromDatabaseEntityToDto(entity);
            if (entityDto != null)
            {
                yield return entityDto;
            }
        }
    }


    public IEnumerable<RegistryObjectDto>? GetRegistryItemsAndRelated(string identifier)
    {
        using var db = _contextFactory.CreateDbContext();

        var singleItem = db.RegistryObjects.AsNoTracking().FirstOrDefault(ro => ro.Id == identifier) ?? db.DocumentEntries.AsNoTracking().FirstOrDefault(ro => ro.DE_UniqueId == identifier);

        if (singleItem == null) return null;

        var association = db.Associations.AsNoTracking().FirstOrDefault(ro => ro.AS_TargetObjectId == singleItem.Id);

        var submissionSet = db.RegistryObjects.AsNoTracking().FirstOrDefault(ro => ro.Id == (association ?? new()).AS_SourceObjectId);

        var resultList = new[] { singleItem, association, submissionSet }.OfType<DbRegistryObject>();
        if (resultList == null) return null;

        return DatabaseMapper.MapFromDatabaseEntityToDto(resultList);
    }

    public RegistryObjectDto? GetSingleRegistryItem(string identifier)
    {
        using var db = _contextFactory.CreateDbContext();

        var entity = db.RegistryObjects.AsNoTracking().FirstOrDefault(ro => ro.Id == identifier) ?? db.DocumentEntries.AsNoTracking().FirstOrDefault(ro => ro.DE_UniqueId == identifier);

        if (entity == null)
        {
            return null;
        }

        return DatabaseMapper.MapFromDatabaseEntityToDto(entity);
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
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        //db.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
        //db.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");

        using var transaction = db.Database.BeginTransaction();


        InsertInBatches(db, documentEntries);
        InsertInBatches(db, submissionSets);
        InsertInBatches(db, associations);

        transaction.Commit();
        return true;
    }

    private static void InsertInBatches<T>(DbContext db, List<T> items) where T : class
    {
        if (items.Count == 0) return;

        db.Set<T>().AddRange(items);
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }

    public bool InsertOrUpdateRegistry(List<RegistryObjectDto> dtos)
    {
        using var db = _contextFactory.CreateDbContext();
        var incomingDbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos).ToList();

        foreach (var e in incomingDbEntities)
            if (string.IsNullOrWhiteSpace(e.Id))
                e.Id = Guid.NewGuid().ToString();

        var documentEntriesToUpload = incomingDbEntities.OfType<DbDocumentEntry>().ToList();
        var submissionSetsToUpload = incomingDbEntities.OfType<DbSubmissionSet>().ToList();
        var associationsToUpload = incomingDbEntities.OfType<DbAssociation>().ToList();

        db.ChangeTracker.AutoDetectChangesEnabled = false;


        DeleteThenInsert(db, db.DocumentEntries, documentEntriesToUpload);
        DeleteThenInsert(db, db.SubmissionSets, submissionSetsToUpload);
        DeleteThenInsert(db, db.Associations, associationsToUpload);
        return true;
    }

    private void DeleteThenInsert<TEntity>(DbContext db, DbSet<TEntity> existingDbSet, List<TEntity> toUpload) where TEntity : DbRegistryObject
    {
        if (toUpload.Count == 0) return;

        // Ensure distinct IDs to avoid duplicates
        toUpload = toUpload
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .GroupBy(x => x.Id!)
            .Select(g => g.Last())
            .ToList();

        var ids = toUpload.Select(x => x.Id!).ToList();
        if (ids.Count == 0) return;

        var existing = existingDbSet
            .Where(x => x.Id != null && ids.Contains(x.Id))
            .ToList();

        if (existing.Count > 0)
        {
            _logger.LogWarning($"Replace: Trying to delete existing {existing.GetType().Name}, count = {existing.Count}");

            existingDbSet.RemoveRange(existing);
            db.SaveChanges();
        }

        existingDbSet.AddRange(toUpload);

        var sql = existingDbSet.ToQueryString(); // for debugging - shows the SQL EF will execute for this batch

        _logger.LogDebug($"{sql}");

        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Ignore — row already gone
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqlEx)
        {
            _logger.LogError(ex,
            "SQLite failure. ErrorCode={ErrorCode}, ExtendedErrorCode={ExtendedErrorCode}",
            sqlEx.SqliteErrorCode,
            sqlEx.SqliteExtendedErrorCode);
            throw;
        }

        db.ChangeTracker.Clear();
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        using var db = _contextFactory.CreateDbContext();
        var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);
        db.RegistryObjects.RemoveRange(db.RegistryObjects);
        db.RegistryObjects.AddRange(dbEntities);
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Ignore — row already gone
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqlEx)
        {
            _logger.LogError(ex,
            "SQLite failure. ErrorCode={ErrorCode}, ExtendedErrorCode={ExtendedErrorCode}",
            sqlEx.SqliteErrorCode,
            sqlEx.SqliteExtendedErrorCode);
            throw;
        }
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