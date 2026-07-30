# Database Migrations

XcaDocumentSource uses **Entity Framework Core** migrations to manage the database schema. Migrations are version-controlled C# files that describe every schema change incrementally. On startup, each registry automatically applies any pending migrations via `context.Database.Migrate()`.

## Backends and Migration Folders

The solution supports two registry backends. Each has its own set of migration files because the generated SQL differs between providers.

| Backend | DbContext | Migration folder |
|---|---|---|
| **PostgreSQL** (production) | `PostGreSqlRegistryDbContext` | `XcaXds.Source/Migrations/PostgreSql/` |

## Prerequisites

The `dotnet ef` CLI tool is required to manage migrations. Install it once globally:

```bash
dotnet tool install --global dotnet-ef
```

All commands below are run from the **repository root**.

---

## Common Operations

### Add a migration (model change)

After modifying an entity class in `XcaXds.Source/Models/DatabaseDtos/`, generate a new migration:

```bash
# SQLite
dotnet ef migrations add <MigrationName> \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context SqliteRegistryDbContext \
  --output-dir Migrations/Sqlite

# PostgreSQL
dotnet ef migrations add <MigrationName> \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context PostGreSqlRegistryDbContext \
  --output-dir Migrations/PostgreSql
```

Use a descriptive `<MigrationName>` such as `AddDocumentStatusField` or `RenamePatientIdColumn`.

---

### Remove the last migration (before it has been applied)

If you need to undo the last generated migration before it has been applied to any database:

```bash
# SQLite
dotnet ef migrations remove \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context SqliteRegistryDbContext

# PostgreSQL
dotnet ef migrations remove \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context PostGreSqlRegistryDbContext
```

> **Warning:** Do not remove a migration that has already been applied to a shared or production database. Revert the database first (see rollback below).

---

### Rollback to a previous migration

To roll back to a specific migration, apply the target migration by name. EF Core will run the `Down()` methods of all migrations applied after it:

```bash
dotnet ef database update <TargetMigrationName> \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context SqliteRegistryDbContext
```

To roll back everything (empty database, no tables):

```bash
dotnet ef database update 0 \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context SqliteRegistryDbContext
```

---

### Add or rename a field

1. Update the entity class (e.g., `DbDocumentEntry.cs`).
2. Generate the migration (see [Add a migration](#add-a-migration-model-change)).
3. EF Core generates an `AddColumn` or `RenameColumn` call automatically.

For a **rename**, EF Core may generate a `DropColumn` + `AddColumn` pair instead of a `RenameColumn`. Check the generated file and replace with an explicit rename if you need to preserve existing data:

```csharp
// In the generated migration Up() method — replace DropColumn/AddColumn with:
migrationBuilder.RenameColumn(
    name: "OldFieldName",
    table: "RegistryObjects",
    newName: "NewFieldName");
```

### Split a field into two fields

This requires a data migration in addition to the schema change. After generating the migration, open the generated file and add a `Sql()` call to backfill the new columns before dropping the old one:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add new columns (generated automatically)
    migrationBuilder.AddColumn<string>("DE_Code", "RegistryObjects", nullable: true);
    migrationBuilder.AddColumn<string>("DE_CodeSystem", "RegistryObjects", nullable: true);

    // 2. Backfill from the old column — adjust SQL per backend
    // SQLite:
    migrationBuilder.Sql(@"
        UPDATE RegistryObjects
        SET DE_Code       = substr(DE_ClassCode, 1, instr(DE_ClassCode, '^') - 1),
            DE_CodeSystem = substr(DE_ClassCode, instr(DE_ClassCode, '^') + 1)
        WHERE DE_ClassCode IS NOT NULL
    ");
    // PostgreSQL (use split_part instead):
    // migrationBuilder.Sql(@"
    //     UPDATE ""RegistryObjects""
    //     SET ""DE_Code""       = split_part(""DE_ClassCode"", '^', 1),
    //         ""DE_CodeSystem"" = split_part(""DE_ClassCode"", '^', 2)
    //     WHERE ""DE_ClassCode"" IS NOT NULL
    // ");

    // 3. Drop the old column (generated automatically, or add manually)
    migrationBuilder.DropColumn("DE_ClassCode", "RegistryObjects");
}
```

> **Note:** SQLite and PostgreSQL use different SQL dialects. If the data migration SQL differs between backends, maintain separate `Sql()` calls in each backend's migration file.

---

### View applied migrations

To inspect which migrations have been applied to a running database:

```bash
dotnet ef migrations list \
  --project XcaXds.Source \
  --startup-project XcaXds.WebService \
  --context SqliteRegistryDbContext
```

Applied migrations are also recorded in the `__EFMigrationsHistory` table in the database itself.

---

## Migrations and Kubernetes Rolling Deploys

During a rolling deployment, old and new pods run against the same database simultaneously for a short period. A schema change that removes or renames a column will cause old pods to fail while they are still running.

To deploy safely, use the **Expand/Contract** pattern across two releases:

**Release N — Expand:** Add new columns only. Both old and new code can coexist.

```csharp
migrationBuilder.AddColumn<string>("NewField", "RegistryObjects", nullable: true);
// Backfill existing rows if needed
migrationBuilder.Sql(@"UPDATE ""RegistryObjects"" SET ""NewField"" = ""OldField""");
```

**Release N+1 — Contract:** Remove old columns once all old pods are gone.

```csharp
migrationBuilder.DropColumn("OldField", "RegistryObjects");
```

This ensures there is never a moment where a running pod references a column that no longer exists.
