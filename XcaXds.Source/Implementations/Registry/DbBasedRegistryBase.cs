using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Implementations.RegistryRepository;

public abstract class DbBasedRegistryBase<TContext> : IRegistry
    where TContext : DbContext
{
    protected readonly ILogger _logger;
    protected readonly IDbContextFactory<TContext> _contextFactory;

    protected DbBasedRegistryBase(ILogger logger, IDbContextFactory<TContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async IAsyncEnumerable<RegistryObjectDto> ReadRegistry()
    {
        var db = await _contextFactory.CreateDbContextAsync();

        await foreach (var entity in db.Set<DbRegistryObject>().AsNoTracking().AsAsyncEnumerable())
        {
            yield return DatabaseMapper.MapFromDatabaseEntityToDto(entity);
        }
    }

    public IEnumerable<RegistryObjectDto> GetRegistryItemsForPatient(PatientId patientIdentifier)
    {
        using var db = _contextFactory.CreateDbContext();
        var entities = db.Set<DbDocumentEntry>()
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

            var singleItem = db.Set<DbRegistryObject>().AsNoTracking().FirstOrDefault(ro => ro.Id == identifier) ??
                             db.Set<DbDocumentEntry>().AsNoTracking().FirstOrDefault(ro => ro.DE_UniqueId == identifier);

            if (singleItem == null) return;

            var association = db.Set<DbAssociation>().AsNoTracking()
                .FirstOrDefault(ro => ro.AS_TargetObjectId == singleItem.Id);

            var submissionSet = db.Set<DbRegistryObject>().AsNoTracking()
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

            var entity = db.Set<DbRegistryObject>().AsNoTracking().FirstOrDefault(ro => ro.Id == identifier) ??
                         db.Set<DbDocumentEntry>().AsNoTracking().FirstOrDefault(ro => ro.DE_UniqueId == identifier);

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

            DeleteThenInsert(db, db.Set<DbDocumentEntry>(), documentEntriesToUpload);
            DeleteThenInsert(db, db.Set<DbSubmissionSet>(), submissionSetsToUpload);
            DeleteThenInsert(db, db.Set<DbAssociation>(), associationsToUpload);
        });
    }

    public OperationResponse WriteRegistry(List<RegistryObjectDto> dtos)
    {
        var response = new OperationResponse();

        ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();
            var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);

            db.Set<DbRegistryObject>().RemoveRange(db.Set<DbRegistryObject>());
            db.Set<DbRegistryObject>().AddRange(dbEntities);

            response = SaveWriteRegistry(db);
        });

        return response;
    }

    public OperationResponse DeleteRegistryItem(string id)
    {
        return ExecuteWithRetry(() =>
        {
            using var db = _contextFactory.CreateDbContext();
            var registryObjectToDelete = db.Set<DbRegistryObject>().FirstOrDefault(ro => ro.Id == id);

            if (registryObjectToDelete != null)
            {
                db.Set<DbRegistryObject>().Remove(registryObjectToDelete);
                db.SaveChanges();
            }
        });
    }

    protected abstract OperationResponse ExecuteWithRetry(Action action, int maxRetries = 3);

    protected virtual OperationResponse SaveWriteRegistry(TContext db)
    {
        db.SaveChanges();
        return OperationResponse.Success("Registry written successfully");
    }

    protected virtual void DeleteThenInsert<TEntity>(TContext db, DbSet<TEntity> existingDbSet, List<TEntity> toUpload)
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

    protected static void InsertInBatches<T>(DbContext db, List<T> items) where T : class
    {
        if (items.Count == 0) return;

        db.Set<T>().AddRange(items);
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }
}
