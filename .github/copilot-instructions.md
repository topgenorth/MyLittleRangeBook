## MyLittleRangeBook – .NET 10 Shooting Logbook

**Project**: A multi-platform logbook application for tracking range trips, Garmin Xero FIT files, and ballistic data. Compiled as single-file self-contained executables on Windows, Linux, and macOS.

**Tech Stack**:
- **.NET 10** with implicit usings and nullable reference types enabled
- **Avalonia** – Cross-platform GUI (disabled tests in CI; see build-simplerangelog.yml)
- **ConsoleAppFramework** – Attribute-routed CLI with command dispatch
- **Spectre.Console** – Rich TUI formatting (CLI only)
- **Dapper** (2.1.72) – Micro-ORM without LINQ; hand-written SQL required
- **DBUp** – Schema migration framework (scripts in MyLittleRangeBook.Sqlite/Scripts/)
- **FluentResults** (4.0.0) – Railway-oriented error handling (all service methods return `Result<T>`)
- **Serilog** – Structured logging to console/debug
- **Garmin.FIT.SDK** – Xero FIT file parsing
- **Microsoft.Data.Sqlite** + **SQLitePCLRaw** – SQLite driver
- **Supabase** cloud database (PostgreSQL backend)

### Build & Restore

**Always run before building**: `dotnet restore` from repository root.

This ensures all transitive dependencies and AOT generators are available. Failure to restore can cause Dapper.AOT codegen errors.

Build commands:
```bash
# Full solution (CLI + GUI, Debug)
dotnet build

# Release build (optimized, trimmed, self-contained)
dotnet build -c Release

# CLI only
dotnet build src/mlrb/MyLittleRangeBook.CLI -c Release

# Publish as single-file executable
dotnet publish src/mlrb/MyLittleRangeBook.CLI -c Release -r win-x64 \
  -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained
```

### Critical: Dapper.AOT Purge Requirement

**If build fails with "Dapper.AOT" or "trimming" errors**, execute **one of**:

PowerShell:
```powershell
Get-ChildItem . -include bin,obj -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
dotnet clean && dotnet restore && dotnet build
```

Bash/Zsh/Linux:
```bash
find . -type d -name "bin" -o -name "obj" | xargs rm -rf
dotnet clean && dotnet restore && dotnet build
```

Or run the provided script: `./src/mlrb/purge-clean.ps1` (Windows) or `./src/mlrb/purge-clean.sh` (Unix).

**Why?** Dapper code generation for AOT compilation caches state in build artifacts. Partial cleans can leave stale artifacts.

### Database Architecture

**Multi-Database Abstraction via Keyed Dependency Injection**:

All data operations are abstracted through interfaces in `MyLittleRangeBook/Services/`:
- `ISimpleRangeLogService` – Query operations (read-heavy)
- `ISimpleRangeEventRepository` – CRUD operations
- `IFirearmsService` – Firearm definitions

Concrete implementations:
- `MyLittleRangeBook.Sqlite/` – SQLite implementations (local default)
- `MyLittleRangeBook.PgSQL/` – PostgreSQL implementations (Supabase)

**Registration** (see `SqliteHelperExtensions.AddMyLittleRangeBookSqlite()`):
```csharp
services.TryAddKeyedSingleton<ISimpleRangeLogService, SqliteSimpleRangeEventService>(SQLITE_KEY);
```

**Never directly reference concrete database classes** in CLI/GUI code—always depend on interfaces.

### Data Layer Rules

1. **All DB operations are async**: Use `async Task<Result<T>>`, never sync I/O.
2. **All operations return FluentResults**: Methods return `Result<T>` or `Result<bool>`, never throw:
   ```csharp
   public async Task<Result<bool>> DeleteAsync(IDbConnection connection, SimpleRangeEvent evt, ...)
   {
       if (evt.RowId is null) return Result.Ok().WithSuccess(new Success("..."));
       try { /* operation */ }
       catch (Exception ex) { return Result.Fail(new Error(ex.Message)); }
   }
   ```

3. **Parameterized Dapper queries only**:
   ```csharp
   const string DeleteSql = "DELETE FROM SimpleRangeEvents WHERE Id = @Id;";
   var cmd = new SqliteCommand(DeleteSql, (SqliteConnection)connection);
   cmd.Parameters.AddWithValue("@Id", evt.Id);
   ```

4. **Model IDs**: 
   - `Id` (Nanoid string, immutable) – for cross-system references
   - `RowId` (nullable long) – SQLite ROWID for internal lookups
   - `Created` / `Modified` (DateTimeOffset) – always set by database via SQL (use `utcnow()` function)

5. **Custom Dapper functions** (registered in `SqliteHelperExtensions.AddFunctions()`):
   - `nanoid()` – generates unique ID
   - `utcnow()` – UTC timestamp

### Environment Configuration

Configuration paths defined in `MyLittleRangeBook/Config/ConfigurationExtensions.cs`:

- **Production**: Reads `appsettings.json` from OS app data folder:
  - Windows: `AppData\Local\MyLittleRangeBook\`
  - Linux/macOS: `~/.local/share/mylittlerangebook/`
- **Development**: Reads from local `appsettings.Development.json` + environment variables

Database filenames auto-suffixed with environment (e.g., `mlrb.Development.db` in dev mode).

### Testing

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=DatabaseTypeHandlerTests"

# Verbose output
dotnet test --logger "console;verbosity=detailed"
```

**Note**: GUI tests disabled in CI (see `.github/workflows/build-simplerangelog.yml` lines 93–98). Core tests use xUnit + in-memory SQLite connections.

### Code Style

- **Nullable refs enabled**: Expect compiler warnings on unhandled nulls—resolve them.
- **ImplicitUsings enabled**: Global namespaces from `GlobalUsings.cs` reduce boilerplate.
- **var keyword**: Use where type is obvious from right side (`var items = GetList()` ✓; `var x = 5` – prefer `int`).
- **Method size**: Keep methods < 20 lines; extract helper methods for clarity.
- **No async void**: All async methods return `Task` or `Task<T>`, never `void`.
- **Editor config**: `.editorconfig` enforces formatting; run `dotnet format` before committing.

### Project Structure Reference

```
src/mlrb/
├── MyLittleRangeBook/            # Core models, interfaces, config
├── MyLittleRangeBook.CLI/        # CLI entry point (Program.cs, ConsoleAppFramework)
├── MyLittleRangeBook.GUI/        # Avalonia GUI
├── MyLittleRangeBook.FIT/        # Garmin FIT parsing (custom error types)
├── MyLittleRangeBook.Sqlite/     # SQLite service implementations + DBUp scripts
├── MyLittleRangeBook.PgSQL/      # PostgreSQL service implementations
├── MyLittleRangeBook.Tests/      # Unit tests (xUnit)
├── SharedControls/               # Reusable UI components
└── fit-reader/                   # Go CLI for FIT parsing (alternative)
```

### Linting & Formatting

```bash
# Check formatting without modifying
dotnet format verify-no-changes

# Auto-fix formatting
dotnet format

# Check specific project
dotnet format src/mlrb/MyLittleRangeBook.CLI
```

Follow `.editorconfig` indentation (4 spaces, Unix line endings for cross-platform builds).

### Debugging Tips

1. **DI not working?** Check `AddMyLittleRangeBookSqlite()` or `AddPostgresHelper()` calls—missing registration = null reference at runtime.
2. **AOT trimming errors?** Run `purge-clean` script—partial builds corrupt Dapper generators.
3. **Environment detection wrong?** Verify `EnvironmentHelper.IsProduction` logic; defaults to Release mode.
4. **Serilog not logging?** Check `.MinimumLevel` configuration in `Program.cs`—dev uses Verbose, prod uses Warning.
5. **Connection string issues?** Validate via `ConfigurationExtensions.DefaultSqliteDatabaseName()` in debugger.

### Key Files to Reference

| File | Purpose |
|------|---------|
| `MyLittleRangeBook.CLI/Program.cs` | Entry point; HostApplicationBuilder setup, DI registration |
| `MyLittleRangeBook.Sqlite/SqliteHelperExtensions.cs` | Keyed DI pattern, SQLite provider initialization |
| `MyLittleRangeBook.Sqlite/SqliteSimpleRangeEventService.cs` | Dapper CRUD pattern example + FluentResults |
| `MyLittleRangeBook/Config/ConfigurationExtensions.cs` | Environment-aware paths, file naming |
| `.github/workflows/build-simplerangelog.yml` | CI/CD behavior—build matrix, publish settings, version computation |
| `MyLittleRangeBook/Models/SimpleRangeEvent.cs` | Core data model (Id + RowId pattern) |
