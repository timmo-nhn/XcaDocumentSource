using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Source.RegistryRepository.PostGreSql;

public class PostGreSqlBasedRegistry : IRegistry
{
    private readonly ILogger<PostGreSqlBasedRegistry> _logger;
    private readonly IDbContextFactory<PostGreSqlRegistryDbContext> _contextFactory;

    public PostGreSqlBasedRegistry(
        ILogger<PostGreSqlBasedRegistry> logger,
        IDbContextFactory<PostGreSqlRegistryDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();
        EnsureDateTimeColumnsAreTimestampWithTimeZone(context);
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

            if (singleItem == null) return;

            var association = db.Associations.AsNoTracking()
                .FirstOrDefault(ro => ro.AS_TargetObjectId == singleItem.Id);

            var submissionSet = db.RegistryObjects.AsNoTracking()
                .FirstOrDefault(ro => ro.Id == (association ?? new()).AS_SourceObjectId);

            var resultList = new[] { singleItem, association, submissionSet }.OfType<DbRegistryObject>();
            itemsToReturn = DatabaseMapper.MapFromDatabaseEntityToDto(resultList);
        });

        return itemsToReturn;
    }

    public RegistryObjectDto? GetSingleRegistryItem(string identifier)
    {
        RegistryObjectDto? registryObjectToReturn = null;

        ExecuteWithRetry(() =>
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

    public OperationResponse AddItemsToRegistry(List<RegistryObjectDto> dtos)
    {
        return ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();
            var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos).ToList();

            foreach (var entity in dbEntities)
            {
                if (string.IsNullOrWhiteSpace(entity.Id))
                {
                    entity.Id = Guid.NewGuid().ToString();
                }
            }

            var documentEntries = dbEntities.OfType<DbDocumentEntry>().ToList();
            var submissionSets = dbEntities.OfType<DbSubmissionSet>().ToList();
            var associations = dbEntities.OfType<DbAssociation>().ToList();

            db.ChangeTracker.AutoDetectChangesEnabled = false;

            using var transaction = db.Database.BeginTransaction();
            InsertInBatches(db, documentEntries);
            InsertInBatches(db, submissionSets);
            InsertInBatches(db, associations);
            transaction.Commit();
        });
    }

    public OperationResponse InsertOrUpdateRegistry(List<RegistryObjectDto> dtos)
    {
        return ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();
            var incomingDbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos).ToList();

            foreach (var entity in incomingDbEntities)
            {
                if (string.IsNullOrWhiteSpace(entity.Id))
                {
                    entity.Id = Guid.NewGuid().ToString();
                }
            }

            var documentEntriesToUpload = incomingDbEntities.OfType<DbDocumentEntry>().ToList();
            var submissionSetsToUpload = incomingDbEntities.OfType<DbSubmissionSet>().ToList();
            var associationsToUpload = incomingDbEntities.OfType<DbAssociation>().ToList();

            db.ChangeTracker.AutoDetectChangesEnabled = false;

            DeleteThenInsert(db, db.DocumentEntries, documentEntriesToUpload);
            DeleteThenInsert(db, db.SubmissionSets, submissionSetsToUpload);
            DeleteThenInsert(db, db.Associations, associationsToUpload);
        });
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
            db.SaveChanges();

            response = OperationResponse.Success("Registry written successfully");
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

    private OperationResponse ExecuteWithRetry(Action action, int maxRetries = 3)
    {
        var error = string.Empty;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                action();
                return OperationResponse.Success($"Operation completed successfully after {attempt} attempt(s)");
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && IsTransient(pgEx.SqlState))
            {
                if (attempt == maxRetries) throw;

                var random = Random.Shared.Next(0, 50);
                var delay = TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt) + random);
                error = ex.ToString();

                _logger.LogWarning(ex,
                    "PostgreSQL transient failure (attempt {Attempt}/{Max}). Retrying in {Delay}ms. SqlState={SqlState}",
                    attempt, maxRetries, delay.TotalMilliseconds, pgEx.SqlState);

                Thread.Sleep(delay);
            }
        }

        return OperationResponse.Failure($"Operation failed after maximum retry attempts {error}");
    }

    private static bool IsTransient(string? sqlState)
    {
        return sqlState is PostgresErrorCodes.SerializationFailure
            or PostgresErrorCodes.DeadlockDetected
            or PostgresErrorCodes.LockNotAvailable
            or PostgresErrorCodes.ConnectionException
            or PostgresErrorCodes.ConnectionDoesNotExist
            or PostgresErrorCodes.ConnectionFailure
            or PostgresErrorCodes.SqlClientUnableToEstablishSqlConnection
            or PostgresErrorCodes.SqlServerRejectedEstablishmentOfSqlConnection;
    }

    private static void EnsureDateTimeColumnsAreTimestampWithTimeZone(PostGreSqlRegistryDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'RegistryObjects'
                      AND column_name = 'DE_CreationTime'
                      AND data_type = 'timestamp without time zone'
                ) THEN
                    ALTER TABLE "RegistryObjects"
                    ALTER COLUMN "DE_CreationTime" TYPE timestamp with time zone USING "DE_CreationTime" AT TIME ZONE 'UTC';
                END IF;
            END
            $$;
            """);

        context.Database.ExecuteSqlRaw("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'RegistryObjects'
                      AND column_name = 'DE_ServiceStartTime'
                      AND data_type = 'timestamp without time zone'
                ) THEN
                    ALTER TABLE "RegistryObjects"
                    ALTER COLUMN "DE_ServiceStartTime" TYPE timestamp with time zone USING "DE_ServiceStartTime" AT TIME ZONE 'UTC';
                END IF;
            END
            $$;
            """);

        context.Database.ExecuteSqlRaw("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'RegistryObjects'
                      AND column_name = 'DE_ServiceStopTime'
                      AND data_type = 'timestamp without time zone'
                ) THEN
                    ALTER TABLE "RegistryObjects"
                    ALTER COLUMN "DE_ServiceStopTime" TYPE timestamp with time zone USING "DE_ServiceStopTime" AT TIME ZONE 'UTC';
                END IF;
            END
            $$;
            """);

        context.Database.ExecuteSqlRaw("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'RegistryObjects'
                      AND column_name = 'DE_SourcePatientInfoBirthTime'
                      AND data_type = 'timestamp without time zone'
                ) THEN
                    ALTER TABLE "RegistryObjects"
                    ALTER COLUMN "DE_SourcePatientInfoBirthTime" TYPE timestamp with time zone USING "DE_SourcePatientInfoBirthTime" AT TIME ZONE 'UTC';
                END IF;
            END
            $$;
            """);

        context.Database.ExecuteSqlRaw("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'RegistryObjects'
                      AND column_name = 'SS_SubmissionTime'
                      AND data_type = 'timestamp without time zone'
                ) THEN
                    ALTER TABLE "RegistryObjects"
                    ALTER COLUMN "SS_SubmissionTime" TYPE timestamp with time zone USING "SS_SubmissionTime" AT TIME ZONE 'UTC';
                END IF;
            END
            $$;
            """);
    }

    private static void InsertInBatches<T>(DbContext db, List<T> items) where T : class
    {
        if (items.Count == 0) return;

        db.Set<T>().AddRange(items);
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }

    private static void DeleteThenInsert<TEntity>(DbContext db, DbSet<TEntity> existingDbSet, List<TEntity> toUpload)
        where TEntity : DbRegistryObject
    {
        if (toUpload.Count == 0) return;

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
            existingDbSet.RemoveRange(existing);
            db.SaveChanges();
        }

        existingDbSet.AddRange(toUpload);
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }
}
