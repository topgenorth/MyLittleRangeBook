# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

MyLittleRangeBook (`mlrb`) is a multi-platform shooting logbook that tracks range trips, Garmin Xero FIT files, and ballistic data. The solution lives at [MyLittleRangeBook.slnx](air-file://jktnaonq2btbg9ueiil8/C:/Users/tom/Code/MyLittleRangeBook/src/mlrb/MyLittleRangeBook.slnx?type=file&root=C%3A) under `src/mlrb/`.

## Build & Test Commands

```bash
# Restore (always run before building)
dotnet restore

# Build (debug)
dotnet build

# Build (release, trimmed)
dotnet build -c Release

# Run all tests
dotnet test

# Run a single test project
dotnet test src/mlrb/MyLittleRangeBook.Tests/MyLittleRangeBook.Tests.csproj

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~SqliteHelperTests"

# Publish CLI as self-contained single file
dotnet publish MyLittleRangeBook.CLI -c Release -r win-x64 \
  -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained
```

### Dapper.AOT build failures

If the build fails with Dapper.AOT or trimming errors, run the purge script from `src/mlrb/`:
- Windows: `./purge-clean.ps1`
- Unix: `./purge-clean.sh`

These errors are caused by stale AOT code-generation artifacts in `bin/` and `obj/` directories.

## Architecture

### Projects

| Project | Role |
|---------|------|
| `MyLittleRangeBook` | Core domain logic, persistence abstractions, models |
| `MyLittleRangeBook.CLI` | CLI entry point (`mlrb`), ConsoleAppFramework commands |
| `MyLittleRangeBook.GUI` | Avalonia GUI (`mlrb-gui`), CommunityToolkit.Mvvm |
| `MyLittleRangeBook.FIT` | Garmin FIT file parsing |
| `SharedControls` | Shared Avalonia components |
| `MyLittleRangeBook.Tests` | Unit/integration tests (xUnit, NSubstitute, Shouldly) |

### Domain Layer

The core library is organised by domain folder, each containing its model, service interface, and SQLite implementation:

- **`EventSourcing/`** – Base `Aggregate`, `DomainEvent`, `IProjector` abstractions
- **`Firearms/`** – Event-sourced `FirearmAggregate` (the canonical ES example)
- **`RangeEvents/`** – `SimpleRangeEvent` CRUD (no event sourcing)
- **`Cartridges/`** – Cartridge/caliber management
- **`Persistence/`** – Dapper abstractions and SQLite connection management
- **`Models/`** – Identity (`MlrbId`), shared value types

### Two Persistence Patterns

**Event sourcing** is used for complex aggregates (currently `FirearmAggregate`):
- Aggregates inherit from `Aggregate`, accumulate state via immutable `DomainEvent` records
- `Raise()` applies an event and queues it as uncommitted; `Apply()` mutates state
- `IProjector` builds read models from the event stream
- Event stream stored in the `EventStreams` SQLite table

**Simple CRUD services** are used for less complex entities (`SimpleRangeEvent`, `Cartridge`, etc.):
- Service interface returns `Result<T>` / `Result` (FluentResults)
- Implementations accept `DapperCommandContext` (wraps connection, transaction, cancellation token)

### Data Layer Rules

1. **All DB operations are async** – return `async Task<Result<T>>`, no sync I/O.
2. **Accept `DapperCommandContext`** – never open connections directly in service methods.
3. **Return FluentResults** – `Result<T>` or `Result`, never throw for expected domain errors.
4. **Never reference concrete DB classes in CLI/GUI** – depend only on interfaces.
5. **Model identity**: `Id` is `MlrbId` (ULID string, immutable); `RowId` is nullable `long` (SQLite ROWID, for upsert tracking); timestamps are `DateTimeOffset` stored as UTC.

### MlrbId (Identity)

All entities use `MlrbId`, a ULID-based identifier (lexicographically sortable by creation time). It supports implicit conversion to/from `string`, `Ulid`, and `byte[]`. See [MlrbId.cs](air-file://jktnaonq2btbg9ueiil8/C:/Users/tom/Code/MyLittleRangeBook/src/mlrb/MyLittleRangeBook/Models/MlrbId.cs?type=file&root=C%3A).

### Dependency Injection

Each domain folder contributes its own `ServiceCollectionExtensions` partial class. DI registration is modular and keyed for multi-database support (currently SQLite only). Start from `MyLittleRangeBook.CLI/Program.cs` to trace the full registration chain.

### Database / Migrations

SQLite with DBUp. Migration scripts are numbered sequentially in [Persistence/Sqlite/Scripts/](air-file://jktnaonq2btbg9ueiil8/C:/Users/tom/Code/MyLittleRangeBook/src/mlrb/MyLittleRangeBook/Persistence/Sqlite/Scripts?type=file&root=C%3A) (`001-*.sql`, `002-*.sql`, …). Add new migrations by appending the next numbered script; DBUp applies them in order. The connection string is configured in `appsettings.json` under `ConnectionStrings:SqliteConnection`. `SqliteHelper` auto-configures WAL mode, foreign keys, and busy timeout.

## Testing

- **Framework**: xUnit
- **Mocking**: NSubstitute (`Substitute.For<T>()`)
- **Assertions**: Shouldly

SQLite integration tests inherit from `SqliteConnectionTestBase`, which provisions a temp on-disk database, runs DBUp migrations, and deletes it on teardown. Use `GetSqliteConnectionAsync()` to get a migrated connection inside a test.

## Key Files

| File | Purpose |
|------|---------|
| `MyLittleRangeBook.CLI/Program.cs` | CLI entry point & DI setup |
| `MyLittleRangeBook.GUI/Program.cs` | GUI entry point & Avalonia init |
| `MyLittleRangeBook/Firearms/FirearmAggregate.cs` | Event sourcing reference implementation |
| `MyLittleRangeBook/Persistence/DapperCommandContext.cs` | DB context pattern |
| `MyLittleRangeBook/Persistence/Sqlite/SqliteHelper.cs` | Connection/migration management |
| `MyLittleRangeBook/Models/MlrbId.cs` | ULID-based identity |
