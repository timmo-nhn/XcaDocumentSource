using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Source.Implementations.RegistryRepository.SqLite;

public class SqliteBasedRegistry : DbBasedRegistryBase<SqliteRegistryDbContext>
{
    private readonly string _connectionString;
    private readonly string _databaseFile;

    public SqliteBasedRegistry(
        ILogger<SqliteBasedRegistry> logger,
        IDbContextFactory<SqliteRegistryDbContext> contextFactory)
        : base(logger, contextFactory)
    {
        _databaseFile = DatabasePathFinder.FindDatabasePath();
        _connectionString = $"Data Source=\"{_databaseFile}\"";

        _logger.LogDebug("Database connection string: {connectionString}", _connectionString);

        using var context = _contextFactory.CreateDbContext();
        EnsureBaselineMigrationRecordedIfPreMigrationDatabase(context);
        context.Database.Migrate();
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        context.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
    }

    public string GetDatabaseFile()
    {
        return _databaseFile;
    }

    protected override OperationResponse ExecuteWithRetry(Action action, int maxRetries = 3)
    {
        var error = string.Empty;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
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

    protected override OperationResponse SaveWriteRegistry(SqliteRegistryDbContext db)
    {
        try
        {
            db.SaveChanges();
            return OperationResponse.Success("Registry written successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return OperationResponse.Success("Registry written successfully");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqlEx)
        {
            _logger.LogError(ex,
                "SQLite failure. ErrorCode={ErrorCode}, ExtendedErrorCode={ExtendedErrorCode}",
                sqlEx.SqliteErrorCode,
                sqlEx.SqliteExtendedErrorCode);

            return OperationResponse.Failure("SQLite failure");
        }
    }

    protected override void DeleteThenInsert<TEntity>(SqliteRegistryDbContext db, DbSet<TEntity> existingDbSet, List<TEntity> toUpload)
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
            _logger.LogWarning("Replace: Trying to delete existing {typeName}, count = {count}", existing.GetType().Name, existing.Count);
            existingDbSet.RemoveRange(existing);
            db.SaveChanges();
        }

        existingDbSet.AddRange(toUpload);
        var sql = existingDbSet.ToQueryString();
        _logger.LogDebug("SQL query for batch operation: {sql}", sql);

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

    private static void EnsureBaselineMigrationRecordedIfPreMigrationDatabase(SqliteRegistryDbContext context)
    {
        var connectionString = context.Database.GetConnectionString()!;
        var baselineMigrationId = context.Database.GetMigrations().First();
        var efCoreVersion = typeof(DbContext).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var existsCmd = connection.CreateCommand();
        existsCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='RegistryObjects'";
        var registryObjectsExists = Convert.ToInt32(existsCmd.ExecuteScalar()) > 0;
        if (!registryObjectsExists)
            return;

        using var idTypeCmd = connection.CreateCommand();
        idTypeCmd.CommandText = "PRAGMA table_info('RegistryObjects')";
        using (var reader = idTypeCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var columnName = reader.GetString(1);
                if (!string.Equals(columnName, "Id", StringComparison.Ordinal))
                    continue;

                var columnType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                if (!string.Equals(columnType, "TEXT", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Detected incompatible legacy SQLite schema in table 'RegistryObjects': column 'Id' has type '{columnType}', expected 'TEXT'. " +
                        "This database cannot be baseline-marked automatically. Migrate the schema manually (or recreate the database) before starting the service.");
                }

                break;
            }
        }

        using var upsertCmd = connection.CreateCommand();
        upsertCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ($migrationId, $productVersion);
            """;
        upsertCmd.Parameters.AddWithValue("$migrationId", baselineMigrationId);
        upsertCmd.Parameters.AddWithValue("$productVersion", efCoreVersion);
        upsertCmd.ExecuteNonQuery();
    }
}