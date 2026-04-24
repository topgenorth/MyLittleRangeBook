# AGENTS.md – My Little Range Book

**Quick Reference for AI Coding Agents**

## Project Overview

**My Little Range Book** is a multi-platform shooting range logging application with desktop (GUI) and CLI interfaces. It parses FIT files (Garmin fitness data), manages firearms/range events, and stores data in SQLite or PostgreSQL. Built on .NET 10 with multi-database abstraction.

## Architecture

### Core Layout
```
src/mlrb/
├── MyLittleRangeBook/              # Core models, interfaces, config (net10.0)
├── MyLittleRangeBook.CLI/          # Command-line interface (ConsoleAppFramework)
├── MyLittleRangeBook.GUI/          # Desktop GUI (Avalonia)
├── MyLittleRangeBook.FIT/          # FIT file parsing (Garmin.FIT.SDK)
├── MyLittleRangeBook.Sqlite/       # SQLite implementations (Dapper, DBUp)
├── MyLittleRangeBook.PgSQL/        # PostgreSQL implementations
├── MyLittleRangeBook.Tests/        # Unit tests (xUnit)
└── fit-reader/                     # Go CLI for parsing FIT files
```

### Multi-Database Abstraction Pattern
The codebase uses **keyed dependency injection** to support multiple database backends:

- **Interface definitions** live in `MyLittleRangeBook/Services/`:
  - `ISimpleRangeLogService` – Query operations
  - `ISimpleRangeEventRepository` – CRUD operations
  - `IFirearmsService` – Firearm management

- **Concrete implementations** per database:
  - `MyLittleRangeBook.Sqlite/Sqlite*.cs` use `Dapper + DBUp + Microsoft.Data.Sqlite`
  - `MyLittleRangeBook.PgSQL/Postgres*.cs` for PostgreSQL

- **Registration pattern** in `SqliteHelperExtensions.AddMyLittleRangeBookSqlite()`:
  ```csharp
  services.TryAddKeyedSingleton<ISimpleRangeLogService, SqliteSimpleRangeEventService>(SQLITE_KEY);
  ```

**Why?** Enables schema-agnostic data layer. CLI defaults to SQLite; GUI can swap databases at runtime.

## Critical Developer Workflows

### Build & Publish
Located in `.github/workflows/build-simplerangelog.yml` – **always consult this** for CI behavior:

```bash
# Local builds (runs both CLI + GUI publish in Release mode)
cd src/mlrb
dotnet restore src/mlrb/MyLittleRangeBook.CLI/MyLittleRangeBook.CLI.csproj
dotnet publish src/mlrb/MyLittleRangeBook.CLI/MyLittleRangeBook.CLI.csproj \
  -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

**Dapper.AOT Critical Issue**: Dapper code generation for AOT compilation is fragile.

```powershell
# ALWAYS run this if build fails with Dapper.AOT errors:
Get-ChildItem . -include bin,obj -Recurse | 
  ForEach-Object { Remove-Item $_.FullName -Force -Recurse }
# Then rebuild
```

Script provided: `./purge-clean.ps1` (PowerShell) or `./purge-clean.sh` (Bash/Zsh)

### Test Execution
GUI tests currently disabled in CI (see `build-simplerangelog.yml` line 93-98).

```bash
# Run unit tests only
dotnet test src/mlrb/MyLittleRangeBook.Tests/MyLittleRangeBook.Tests.csproj -c Release
```

Test patterns in `MyLittleRangeBook.Tests/Sqlite/` use xUnit + in-memory SQLite connections.

## Project-Specific Conventions

### Error Handling: FluentResults Pattern
**Every service method returns `Result<T>` or `Result<bool>`**, never throws:

```csharp
// From SqliteSimpleRangeEventService.cs
public async Task<Result<bool>> DeleteAsync(IDbConnection connection, SimpleRangeEvent evt, ...)
{
    if (evt.RowId is null)
    {
        var reason = new Success($"SimpleRangeEvent `{evt.Id}` does not exist.");
        reason.WithMetadata("Id", evt.Id);
        return Result.Ok().WithSuccess(reason);
    }
    try { /* ... */ }
    catch (Exception ex) { return Result.Fail(new Error(ex.Message)); }
}
```

**Action**: Always check `result.IsFailed` before accessing `.Value`. Use `.WithMetadata()` for context.

### Data Access: Dapper with Raw SQL
- **No LINQ to SQL or EF Core** – uses Dapper + hand-written SQL strings
- SQL queries stored as `const string` fields in service classes
- Parameters via `@ParameterName` syntax (Dapper positional mapping)
- Custom Dapper functions registered in `SqliteHelperExtensions.AddFunctions()`:
  - `nanoid()` – generates unique IDs
  - `utcnow()` – current UTC timestamp

**Why?** Enables AOT compilation (trim-friendly) and explicit control over schema.

### Models & IDs
Located in `MyLittleRangeBook/Models/`:

- `SimpleRangeEvent` – Primary model (Event ID is `Nanoid`, RowId is SQLite ROWID)
- `Firearm` – Firearm definitions

**Field convention**: 
- `Id` (Nanoid, immutable) for cross-system references
- `RowId` (nullable long, SQLite ROWID) for internal references
- `Created` / `Modified` (DateTimeOffset) timestamps

### Configuration & Environments
`ConfigurationExtensions.cs` defines environment-aware paths:

- **Production**: Reads `appsettings.json` from `~/.local/share/MyLittleRangeBook/` (Linux) or `AppData\Local\MyLittleRangeBook` (Windows)
- **Development**: Reads `appsettings.Development.json` + environment variables
- Database name suffixed with environment (e.g., `mlrb.Development.db`)

Register configs: `builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)`

## Integration Points & Dependencies

### External Packages (Key)
| Package | Purpose | Notes |
|---------|---------|-------|
| **Dapper** (2.1.72) | ORM | No code generation; hand-mapped SQL |
| **FluentResults** (4.0.0) | Error handling | Railway-oriented programming |
| **Garmin.FIT.SDK** (21.195.0) | FIT parsing | C# wrapper; fit-reader (Go) alternative |
| **ConsoleAppFramework** (5.7.13) | CLI commands | Attribute-based command routing |
| **Serilog** (4.3.1) | Logging | Structured logs to console/debug |
| **DBUp** (5.0.41+) | Schema migrations | SQL scripts in `MyLittleRangeBook.Sqlite/Scripts/` |
| **Microsoft.Data.Sqlite** (10.0.5) | SQLite driver | Used with SQLitePCLRaw |
| **Spectre.Console** (0.55.0) | TUI formatting | Rich console output (CLI) |

### Cross-Component Communication
- **CLI → Sqlite**: Directly injects `ISimpleRangeLogService` keyed singleton
- **GUI → Sqlite**: Instantiates separate connection; see `DatabaseHelper.cs` export/import methods
- **FIT → Models**: `MyLittleRangeBook.FIT/` parses Garmin data → `SimpleRangeEvent` models
- **fit-reader (Go)** → CLI: Subprocess call (if needed); see `import-rangetrips.ps1`

## Key Files for Pattern Reference

| File | Pattern |
|------|---------|
| `MyLittleRangeBook.CLI/Program.cs` | HostApplicationBuilder setup, DI registration, environment handling |
| `MyLittleRangeBook.Sqlite/SqliteHelperExtensions.cs` | Keyed DI pattern, SQLite provider init |
| `MyLittleRangeBook.Sqlite/SqliteSimpleRangeEventService.cs` | Dapper CRUD, FluentResults, raw SQL |
| `MyLittleRangeBook/Config/ConfigurationExtensions.cs` | Environment-aware paths, file injection |
| `MyLittleRangeBook.FIT/FailedToLoadFitFileError.cs` | FluentResults custom error types |

## Debugging Tips

1. **AOT Compilation Issues**: Always purge `bin/obj` and run `dotnet restore` first (see Dapper.AOT section)
2. **Environment Variables**: Check `EnvironmentHelper.IsProduction` logic; defaults to Release mode detection
3. **Database Path**: Verify via `ConfigurationExtensions.DefaultSqliteDatabasePath` in debugger
4. **DI Registration**: Look for `AddMyLittleRangeBookSqlite()` calls; missing registration = runtime null reference
5. **Serilog**: Filter verbosity with `.MinimumLevel.Information()` etc.; development uses Verbose

## Notes for Contributors

- **Net 10.0 + Nullable refs enabled**: Expect compiler warnings on unhandled nulls
- **ImplicitUsings**: Global namespaces from `GlobalUsings.cs` reduce boilerplate
- **Editor config**: `.editorconfig` enforces formatting; linters configured per project
- **No async void**: All async methods return `Task` or `Task<T>`

