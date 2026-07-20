## MyLittleRangeBook – .NET 10 Shooting Logbook

**Project**: A multi-platform logbook application for tracking range trips, Garmin Xero FIT files, and ballistic data. Targets include a CLI (Windows/Linux) and an Avalonia-based GUI.

**Tech Stack**:
- **.NET 10** with implicit usings and nullable reference types enabled.
- **Avalonia** – Cross-platform UI framework (GUI only).
- **ConsoleAppFramework** – Attribute-routed CLI with command dispatch.
- **Spectre.Console** – Rich TUI formatting (CLI only).
- **Dapper** – Micro-ORM using custom `DapperCommand` and `DapperCommandContext` abstractions.
- **Event Sourcing** – Pattern used for domain aggregates (e.g., `Firearm`), including `Aggregate`, `DomainEvent`, and `Projector` components.
- **DBUp** – Schema migration framework (scripts in `MyLittleRangeBook/Persistence/Sqlite/Scripts/`).
- **FluentResults**  – Railway-oriented error handling (all service methods return `Result<T>`).
- **Serilog** – Structured logging.
- **Microsoft.Data.Sqlite** – SQLite driver for local persistence.

### Build & Restore

**Always run before building**: `dotnet restore` from repository root.

Build commands:
```bash
# Full solution (CLI + GUI, Debug)
dotnet build

# Release build (optimized, trimmed, self-contained)
dotnet build -c Release

# Publish as single-file executable (example for CLI)
dotnet publish MyLittleRangeBook.CLI -c Release -r win-x64 \
  -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained
```

### Critical: Dapper.AOT Purge Requirement

**If build fails with "Dapper.AOT" or "trimming" errors**, execute the provided purge script from `src/mlrb/`:
- Windows: `./purge-clean.ps1`
- Unix: `./purge-clean.sh`

**Why?** Dapper code generation for AOT compilation caches state in build artifacts. Partial cleans can leave stale artifacts that cause compilation failures.

### Database Architecture

**Multi-Database Abstraction via Keyed Dependency Injection**:

All data operations are abstracted through interfaces and implemented using Dapper.
- `ISimpleRangeEventService` – Management of simple range log events.
- `IFirearmsService` – Projection-based firearms management.
- `ISqliteHelper` – Provides scoped SQLite connections.

Concrete implementations and persistence logic are located in `MyLittleRangeBook/Persistence/`.

**Never directly reference concrete database classes** in CLI/GUI code—always depend on interfaces.

### Data Layer Rules

1. **All DB operations are async**: Use `async Task<Result<T>>`, never sync I/O.
2. **Use DapperCommandContext**: Methods should accept a `DapperCommandContext` which encapsulates the connection, transaction, and cancellation token.
3. **All operations return FluentResults**: Methods return `Result<T>` or `Result`, never throw for expected domain errors.
   ```csharp
   public async Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent evt)
   {
       try { 
           DapperCommandContext ctx = context with { Arguments = new { evt.Id } };
           await Commands.s_delete.ExecuteAsync(ctx).ConfigureAwait(false);
           return Result.Ok();
       }
       catch (Exception ex) { return Result.Fail(ex.ToError()); }
   }
   ```
4. **Model IDs**: 
   - `Id` (`MlrbId` / ULID string) – Immutable identifier for cross-system references.
   - `RowId` (nullable long) – SQLite `ROWID` for internal lookups and upsert tracking.
   - `Created` / `Modified` (DateTimeOffset) – Managed as UTC.

### Event Sourcing

For domain aggregates like `Firearm`, the system uses an Event Sourcing pattern:
- **Aggregate**: Base class for domain entities that accumulate state from events.
- **DomainEvent**: Immutable records representing state changes.
- **Projector**: Responsible for updating read models from the event stream.
- **AggregateRepository**: Handles loading/saving aggregates from/to the event store.

### Project Structure Reference

```
src/mlrb/
├── MyLittleRangeBook/            # Core logic, models, and persistence
│   ├── Cartridges/               # Cartridge and caliber management
│   ├── Config/                   # Environment and application configuration
│   ├── EventSourcing/            # Base ES abstractions (Aggregate, DomainEvent)
│   ├── Firearms/                 # Firearm aggregate, events, and service
│   ├── IO/                       # Import/Export logic (CSV, FIT)
│   ├── MlrbAssets/               # Handling of binary assets (images, files)
│   ├── Models/                   # Domain models and identity (MlrbId)
│   ├── Persistence/              # Dapper and SQLite implementation
│   │   └── Sqlite/               # SQLite specific logic and DBUp scripts
│   └── RangeEvents/              # Simple range event logic
├── MyLittleRangeBook.CLI/        # CLI entry point and commands
├── MyLittleRangeBook.GUI/        # Avalonia GUI project
├── MyLittleRangeBook.GUI.Tests/  # Tests for the GUI project
├── MyLittleRangeBook.FIT/        # Garmin FIT parsing (C# implementation)
├── MyLittleRangeBook.Tests/      # Core and Service unit tests
├── SharedControls/               # Shared Avalonia UI components
├── SharedControlsTests/          # Tests for shared UI components
├── fit-reader/                   # Go-based FIT parsing utility
├── hatcher/                      # AI prompt templates and samples
├── sql-scripts/                  # Utility SQL scripts for maintenance
└── supabase/                     # Supabase configuration and migrations
```

### Code Style

- **Nullable refs enabled**: Mandatory handling of nulls.
- **ImplicitUsings enabled**: Reduces boilerplate for common namespaces.
- **var keyword**: Use where type is obvious from the right side.
- **Method size**: Aim for small, focused methods (< 25 lines).
- **No async void**: All async methods must return `Task` or `ValueTask`.

### Debugging Tips

1. **DI issues?** Check registration in `ServiceCollectionExtensions` partial classes (e.g., in `MyLittleRangeBook/Firearms/`, `MyLittleRangeBook/Persistence/Sqlite/`, etc.).
2. **AOT errors?** Run the `purge-clean` script immediately.
3. **Connection issues?** Verify `SqliteConnection` string in `appsettings.json` or environment variables.

### Key Files to Reference

| File | Purpose |
|------|---------|
| `MyLittleRangeBook.CLI/Program.cs` | CLI entry point and DI registration |
| `MyLittleRangeBook.GUI/Program.cs` | GUI entry point and Avalonia initialization |
| `MyLittleRangeBook/Persistence/Sqlite/SqliteHelperExtensions.cs` | SQLite provider setup |
| `MyLittleRangeBook/Firearms/FirearmAggregate.cs` | Event Sourcing aggregate example |
| `MyLittleRangeBook/Persistence/DapperCommandContext.cs` | Context pattern for database operations |
| `MyLittleRangeBook/Models/MlrbId.cs` | ULID-based identity implementation |
