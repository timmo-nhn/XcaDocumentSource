using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Source.Implementations.RegistryRepository.PostGreSql;

public class PostGreSqlBasedRegistry : DbBasedRegistryBase<PostGreSqlRegistryDbContext>
{
    public PostGreSqlBasedRegistry(
        ILogger<PostGreSqlBasedRegistry> logger,
        IDbContextFactory<PostGreSqlRegistryDbContext> contextFactory)
        : base(logger, contextFactory)
    {
        using var context = _contextFactory.CreateDbContext();
        EnsureBaselineMigrationRecordedIfPreMigrationDatabase(context);
        context.Database.Migrate();
        EnsureDateTimeColumnsAreTimestampWithTimeZone(context);
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

    /// <summary>
    /// Databases created before EF Core migrations were introduced have tables but no
    /// __EFMigrationsHistory table or entries. Migrate() would fail with "relation already exists".
    /// Detect this case and mark the baseline migration as already applied so Migrate()
    /// becomes a no-op for the initial schema and only applies future deltas.
    ///
    /// Uses a standalone connection that is fully closed before Migrate() runs — if we
    /// leave EF Core's own connection open, Migrate() treats it as externally owned and
    /// the freshly inserted history row may not be visible to its internal lookup.
    /// </summary>
    private static void EnsureBaselineMigrationRecordedIfPreMigrationDatabase(PostGreSqlRegistryDbContext context)
    {
        var connectionString = context.Database.GetConnectionString()!;
        var baselineMigrationId = context.Database.GetMigrations().First();

        using var connection = new NpgsqlConnection(connectionString);
        try
        {
            connection.Open();
        }
        catch (PostgresException ex) when (ex.SqlState == "3D000")
        {
            // Database doesn't exist yet. Let Migrate() handle first-time database creation.
            return;
        }

        // If the main schema tables don't exist yet, this is a fresh database — let Migrate() handle it.
        using var existsCmd = connection.CreateCommand();
        existsCmd.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'RegistryObjects')";
        if (!(bool)existsCmd.ExecuteScalar()!)
            return;

        // Tables exist. Ensure __EFMigrationsHistory exists and contains the baseline entry.
        // This is safe to run on any database state: already-migrated DBs hit ON CONFLICT DO NOTHING.
        var efCoreVersion = typeof(DbContext).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";

        using var upsertCmd = connection.CreateCommand();
        upsertCmd.CommandText = $"""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('{baselineMigrationId}', '{efCoreVersion}')
            ON CONFLICT DO NOTHING;
            """;
        upsertCmd.ExecuteNonQuery();

        // Connection disposed here — Migrate() opens a fresh connection and sees the history row.
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

}
