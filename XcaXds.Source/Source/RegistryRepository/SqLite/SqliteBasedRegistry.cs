using Hl7.Fhir.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source.RegistryRepository.SqLite;

public class SqliteBasedRegistry : IRegistry
{
    private readonly ILogger<SqliteBasedRegistry> _logger;
    private readonly IDbContextFactory<SqliteRegistryDbContext> _contextFactory;

    private readonly string _connectionString;
    private readonly string _databaseFile;

    public SqliteBasedRegistry(ILogger<SqliteBasedRegistry> logger,
        IDbContextFactory<SqliteRegistryDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;

        _databaseFile = DatabasePathFinder.FindDatabasePath();

        _connectionString = $"Data Source=\"{_databaseFile}\"";

        _logger.LogDebug($"Database connection string: {_connectionString}");
        using var context = _contextFactory.CreateDbContext();

        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        context.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
    }

    public string GetDatabaseFile()
    {
        return _databaseFile;
    }

    public async IAsyncEnumerable<RegistryObjectDto> ReadRegistry()
    {
        var db = await _contextFactory.CreateDbContextAsync();

        await foreach (var entity in db.RegistryObjects.AsNoTracking().AsAsyncEnumerable())
        {
            yield return DatabaseMapper.MapFromDatabaseEntityToDto(entity);
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
        IEnumerable<RegistryObjectDto>? itemsToReturn = null;

        ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();

            var singleItem = db.RegistryObjects.AsNoTracking().FirstOrDefault(ro => ro.Id == identifier) ??
                             db.DocumentEntries.AsNoTracking().FirstOrDefault(ro => ro.DE_UniqueId == identifier);

            if (singleItem != null)
            {
                var association = db.Associations.AsNoTracking()
                    .FirstOrDefault(ro => ro.AS_TargetObjectId == singleItem.Id);

                var submissionSet = db.RegistryObjects.AsNoTracking()
                    .FirstOrDefault(ro => ro.Id == (association ?? new()).AS_SourceObjectId);

                var resultList = new[] { singleItem, association, submissionSet }.OfType<DbRegistryObject>();

                if (resultList != null)
                {
                    itemsToReturn = DatabaseMapper.MapFromDatabaseEntityToDto(resultList);
                }
            }
        });

        return itemsToReturn;
    }

    public RegistryObjectDto? GetSingleRegistryItem(string identifier)
    {
        RegistryObjectDto? registryObjectToReturn = null;

        var response = ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();

            var entity = db.RegistryObjects.AsNoTracking().FirstOrDefault(ro => ro.Id == identifier) ??
                         db.DocumentEntries.AsNoTracking().FirstOrDefault(ro => ro.DE_UniqueId == identifier);

            if (entity != null)
            {
                registryObjectToReturn = DatabaseMapper.MapFromDatabaseEntityToDto(entity);
            }
        });

        return registryObjectToReturn;
    }

    public OperationResponse UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        return ExecuteWithRetry(() =>
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
            db.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking; // mostly for queries, harmless here

            using var transaction = db.Database.BeginTransaction();

            InsertInBatches(db, documentEntries);
            InsertInBatches(db, submissionSets);
            InsertInBatches(db, associations);

            transaction.Commit();
        });
    }

    public OperationResponse InsertOrUpdateRegistry(List<RegistryObjectDto> dtos)
    {
        var response = ExecuteWithRetry(() =>
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
        });

        return response;
    }

    public OperationResponse WriteRegistry(List<RegistryObjectDto> dtos)
    {
        var response = new OperationResponse();

        ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();
            var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);
            db.RegistryObjects.RemoveRange(db.RegistryObjects);
            db.RegistryObjects.AddRange(dbEntities);
            try
            {
                db.SaveChanges();
                response = OperationResponse.Success("Registry written successfully");
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
                response = OperationResponse.Failure("SQLite failure");
            }
        });

        return response;
    }

    public OperationResponse DeleteRegistryItem(string id)
    {
        return ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();
            var registryObjectToDelete = db.RegistryObjects.FirstOrDefault(ro => ro.Id == id);

            if (registryObjectToDelete != null)
            {
                db.RegistryObjects.Remove(registryObjectToDelete);
                db.SaveChanges();
            }
        });
    }

    public OperationResponse ExecuteWithRetry(Action action, int maxRetries = 3)
    {
        var error = string.Empty;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                action();
                return OperationResponse.Success($"Operation completed successfully after {attempt} attempt(s)");
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqlEx && IsTransient(sqlEx))
            {
                if (attempt == maxRetries)
                    throw;

                // Jitter delay
                var random = Random.Shared.Next(0, 50);
                var delay = TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt) + random);
                error = ex.ToString();
                _logger.LogWarning(ex,
                    "SQLite transient failure (attempt {Attempt}/{Max}). Retrying in {Delay}ms. Code={Code}, Extended={Extended}, HRESULT={Hresult}",
                    attempt, maxRetries, delay.TotalMilliseconds,
                    sqlEx.SqliteErrorCode,
                    sqlEx.SqliteExtendedErrorCode,
                    sqlEx.ErrorCode);

                _logger.LogWarning("Exception JSON representation\n{ex}", JsonSerializer.Serialize(sqlEx));

                Thread.Sleep(delay);
            }
        }

        return OperationResponse.Failure($"Operation failed after maximum retry attempts {error}");

        static bool IsTransient(SqliteException ex)
        {
            return ex.SqliteErrorCode is 5 or 6 or 10;
        }
    }

    private static void InsertInBatches<T>(DbContext db, List<T> items) where T : class
    {
        if (items.Count == 0) return;

        db.Set<T>().AddRange(items);
        db.SaveChanges();
        db.ChangeTracker.Clear();
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
            _logger.LogWarning(
                $"Replace: Trying to delete existing {existing.GetType().Name}, count = {existing.Count}");

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
}