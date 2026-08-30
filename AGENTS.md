# AGENTS: How to be productive in the MyLittleRangeBook repo

This is the **canonical** guidance file for AI coding agents working on MyLittleRangeBook.
(`CLAUDE.md` and `.github/copilot-instructions.md` should defer to this file.)

MyLittleRangeBook (`mlrb`) is a multi-platform shooting logbook that tracks range trips,
Garmin Xero FIT files, and ballistic data. The .NET solution lives at
`src/mlrb/MyLittleRangeBook.slnx`. Run `dotnet` commands from `src/mlrb/` unless noted;
the SDK is pinned by `src/mlrb/global.json`.

## 1) Big picture (what to read first)

- `src/mlrb/MyLittleRangeBook.CLI/Program.cs` — host/DI setup and logging; the best entry
  point for tracing how services are wired.
- Core library `src/mlrb/MyLittleRangeBook/` is organised **by domain folder**, not by layer.
  Each domain folder holds its model, service interface, SQLite implementation, and a
  partial `ServiceCollectionExtensions`.

## 2) Projects

| Project | Role |
|---------|------|
| `MyLittleRangeBook` | Core domain logic, persistence, models |
| `MyLittleRangeBook.CLI` | CLI entry point (`mlrb`), ConsoleAppFramework commands |
| `MyLittleRangeBook.GUI` | Avalonia GUI (`mlrb-gui`), CommunityToolkit.Mvvm |
| `MyLittleRangeBook.FIT` | Garmin FIT file parsing |
| `SharedControls` | Shared Avalonia components |
| `MyLittleRangeBook.Tests` | Unit/integration tests (xUnit, NSubstitute, Shouldly) |
| `MyLittleRangeBook.GUI.Tests` / `SharedControlsTests` | GUI-side tests (disabled in CI) |

Other folders under `src/mlrb/`: `sql-scripts/`, `supabase/`, `hatcher/`, `fit-reader/`.

## 3) Architecture & why

- **Event driven and event sourcing code**. This project is meant to be entirely event driven.
- **Domain-organised core.** Persistence is *inside* the core project under
  `MyLittleRangeBook/Persistence/`. There is **no separate `.Sqlite` or `.PgSQL` provider
  project, and no Postgres implementation** — SQLite only today.
- **All DB work is async and returns FluentResults.** Methods are `Task<Result<T>>` /
  `Task<Result>`; never throw for expected domain errors.
- **Dapper with handwritten SQL** (no LINQ). Dapper AOT codegen is used; stale `bin/`/`obj/`
  artifacts break builds.
- **DI is modular and keyed.** Each domain folder contributes a partial
  `ServiceCollectionExtensions` (e.g. `RegisterRangeEventStuff`, `RegisterFirearmEventSourcing`,
  `RegisterNotes`). SQLite registration is `RegisterMyLittleRangeBookSqlite(services, configuration)`
  in `Persistence/Sqlite/SqliteHelperExtensions.cs`, using keyed registrations via
  `SqliteHelperExtensions.DI_KEY`.

## 4) Two persistence patterns

**Event sourcing** — used for complex aggregates (canonical example: `Firearms/FirearmAggregate.cs`):
- Aggregates inherit from `Aggregate` and accumulate state via immutable `DomainEvent` records.
- `Raise()` applies an event and queues it as uncommitted; `Apply()` mutates state.
- `IProjector` builds read models; the event stream is stored in the `EventStreams` table.
- Base abstractions live in `MyLittleRangeBook/EventSourcing/`.

**Simple CRUD services** — used for less complex entities (`SimpleRangeEvent`, `Cartridge`, etc.):
- Service interface returns `Result<T>` / `Result`.
- Implementations accept a `DapperCommandContext` (wraps connection, transaction, cancellation).

## 5) Concrete patterns to follow

- **Error handling:** return `Result<T>` instead of throwing.
- **Identity:** `Id` is `MlrbId` — a ULID string (lexicographically sortable, immutable),
  with implicit conversion to/from `string`, `Ulid`, and `byte[]` (see `Models/MlrbId.cs`).
  `RowId` is a nullable `long` (SQLite ROWID, for upsert tracking). Timestamps are
  `DateTimeOffset` stored as UTC.
- **Never open connections directly in service methods** — accept a `DapperCommandContext`,
  or, in CLI/GUI code, obtain a scope via `SqliteHelper.GetScopedDatabaseConnectionAsync()`
  (returns a `ScopedSqliteConnection`).
- **Parameterized Dapper SQL only.** Use named parameters, e.g.:
  ```csharp
  const string DeleteSql = "DELETE FROM SimpleRangeEvents WHERE Id = @Id;";
  await ctx.Connection.ExecuteAsync(DeleteSql, new { evt.Id }, ctx.Transaction);
  ```
- **Custom SQLite functions** registered by `SqliteConnection.AddFunctions()`
  (`Persistence/Sqlite/SqliteHelperExtensions.cs`): `nanoid()` (actually returns a ULID string)
  and `utcnow()` (UTC `DateTimeOffset` in round-trip "O" format).

## 6) Build / test / debug workflows

Run from `src/mlrb/`:

```bash
dotnet restore                       # always run first (avoids Dapper.AOT generator errors)
dotnet build                         # debug, whole solution
dotnet build -c Release              # release, trimmed
dotnet test                          # all tests
dotnet test --filter "FullyQualifiedName~SqliteHelperTests"   # focus one test/class
dotnet publish MyLittleRangeBook.CLI -c Release -r win-x64 \
  -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained
```

- **Dapper.AOT / trimming errors:** run the purge script from `src/mlrb/`
  (`./purge-clean.ps1` on Windows, `./purge-clean.sh` on Unix), or remove `bin/` and `obj/`
  then `dotnet clean && dotnet restore && dotnet build`. This is a frequent failure after
  changing the data layer.
- GUI tests are disabled in CI.

## 7) Testing

- xUnit + NSubstitute (`Substitute.For<T>()`) + Shouldly.
- SQLite integration tests inherit from `SqliteConnectionTestBase`, which provisions a temp
  on-disk DB, runs DBUp migrations, and deletes it on teardown. Use `GetSqliteConnectionAsync()`
  to get a migrated connection.

## 8) Database / migrations

SQLite with DBUp. Migration scripts are numbered sequentially in
`MyLittleRangeBook/Persistence/Sqlite/Scripts/` (`001-*.sql`, `002-*.sql`, …). Add a migration
by appending the next numbered script; DBUp applies them in order. The connection string is in
`appsettings.json` under `ConnectionStrings:SqliteConnection`. `SqliteHelper` auto-configures
WAL mode, foreign keys, and busy timeout. `Config/ConfigurationExtensions.cs` (and
`DefaultSqliteDatabaseName()`) determine file locations — check there when debugging connections.

## 9) Agent checklist for a typical change

- Read the domain folder's model + interface before editing its SQLite implementation.
- Preserve async / FluentResults signatures and parameterized SQL.
- For DB schema changes, add a numbered DBUp script under `Persistence/Sqlite/Scripts/` and a
  migration test.
- Run `dotnet restore && dotnet build && dotnet test` from `src/mlrb/`; run purge-clean if you
  hit Dapper.AOT/trimming errors.

## 10) Key files

| File (under `src/mlrb/`) | Purpose |
|---|---|
| `MyLittleRangeBook.CLI/Program.cs` | CLI entry point & DI wiring |
| `MyLittleRangeBook.GUI/Program.cs` | GUI entry point & Avalonia init |
| `MyLittleRangeBook/Firearms/FirearmAggregate.cs` | Event-sourcing reference implementation |
| `MyLittleRangeBook/Persistence/DapperCommandContext.cs` | DB context pattern |
| `MyLittleRangeBook/Persistence/Sqlite/SqliteHelper.cs` | Connection/migration management |
| `MyLittleRangeBook/Persistence/Sqlite/SqliteHelperExtensions.cs` | DI + custom SQL functions |
| `MyLittleRangeBook/Models/MlrbId.cs` | ULID-based identity |
| `MyLittleRangeBook/RangeEvents/SimpleRangeEvent.cs` | Id/RowId/timestamp conventions |

## 11) Safety rules for agents

- Avoid hardcoding secrets; use environment variables or `appsettings.*`.
- Preserve the FluentResults error-return style; do not convert to exceptions.
- Never reference concrete DB classes from CLI/GUI — depend only on interfaces.
- Keep changes small and run tests locally. If you change SQL schema, add DBUp scripts and
  migration tests.

---
When in doubt, open the files in section 10 and run `dotnet restore && dotnet build && dotnet test`
from `src/mlrb/` before opening a PR.
