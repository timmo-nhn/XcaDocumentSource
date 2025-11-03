using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;
using XcaXds.Source.Services;

namespace XcaXds.Source.Source;

public class SqliteBasedRegistry : IRegistry
{
    private readonly ILogger<SqliteBasedRegistry> _logger;
    private readonly object _lock = new();
    private readonly IDbContextFactory<SqliteRegistryDbContext> _contextFactory;

    private readonly string _connectionString;
    private readonly string _registryPath;
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
    }

    public string GetDatabseFile()
    {
        return _databaseFile;
    }

    public List<RegistryObjectDto> ReadRegistry()
    {
        using var db = _contextFactory.CreateDbContext();
        var registryObjects = DatabaseMapper.MapFromDatabaseEntityToDto(db.RegistryObjects.ToList());
        return registryObjects;
    }

    public bool UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        using var db = _contextFactory.CreateDbContext();
        var dbEntities = DatabaseMapper.MapFromDtoToDatabaseEntity(dtos);

        foreach (var e in dbEntities)
        {
            if (string.IsNullOrWhiteSpace(e.Id))
                e.Id = Guid.NewGuid().ToString();
        }

        var documentEntries = dbEntities.OfType<DbDocumentEntry>().ToList();
        var submissionSets = dbEntities.OfType<DbSubmissionSet>().ToList();
        var associations = dbEntities.OfType<DbAssociation>().ToList();

        db.ChangeTracker.AutoDetectChangesEnabled = false;

        using var transaction = db.Database.BeginTransaction();

        if (documentEntries.Count > 0)
            db.BulkInsert(documentEntries);

        if (submissionSets.Count > 0)
            db.BulkInsert(submissionSets);

        if (associations.Count > 0)
            db.BulkInsert(associations);

        transaction.Commit();

        return true;
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