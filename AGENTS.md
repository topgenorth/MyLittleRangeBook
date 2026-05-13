# MyLittleRangeBook Agent Guide

## Overview
MyLittleRangeBook is a cross-platform .NET 10 application for tracking shooting range logs, integrating Garmin Xero FIT files. It uses a CLI-first approach with Avalonia GUI, Dapper for data access (SQLite/PostgreSQL), and FluentResults for error handling.

## Architecture
- **Multi-project solution**: Core models in `MyLittleRangeBook/`, CLI in `MyLittleRangeBook.CLI/`, GUI in `MyLittleRangeBook.GUI/`, database implementations in `MyLittleRangeBook.Sqlite/` and `MyLittleRangeBook.PgSQL/`.
- **Dependency Injection**: Keyed services for database abstraction (e.g., `ISimpleRangeLogService` registered as "sqlite" or "pgsql").
- **Data Flow**: FIT files → Parsing (Garmin.FIT.SDK) → Domain models → Async persistence via Dapper → SQLite/PostgreSQL.

## Key Patterns
- **Async-First**: All DB ops return `Task<Result<T>>`; use `await` everywhere. Example: `await service.UpsertAsync(connection, event)`.
- **FluentResults**: Never throw exceptions; return `Result.Ok(value)` or `Result.Fail(new Error("msg"))`. Enrich errors with metadata.
- **Dapper Queries**: Parameterized only; use `connection.ExecuteAsync(sql, param)`. Custom functions: `nanoid()`, `utcnow()`.
- **Model IDs**: `Id` (Nanoid string, immutable) for cross-system refs; `RowId` (long?) for SQLite internals.
- **CLI Commands**: Use ConsoleAppFramework attributes; inject services via DI. Example: `[Command("import-fit")] async Task ImportFitAsync(string filePath, [FromServices] IXeroShotSessionParser parser)`.

## Critical Workflows
- **Build**: Always `dotnet restore` first; purge bin/obj on Dapper.AOT errors with `src/mlrb/purge-clean.ps1`.
- **Database Setup**: Auto-creates SQLite in user app data (e.g., `~/.local/share/mylittlerangebook/mlrb.Development.db`).
- **FIT Import**: Parse via `IXeroShotSessionParser`, validate, map to `SimpleRangeEvent`, upsert idempotently.
- **Testing**: `dotnet test`; GUI tests disabled in CI.

## Integration Points
- **Garmin FIT**: Use `Garmin.FIT.SDK` for parsing; handle device-specific enums and timestamps.
- **Supabase**: PostgreSQL backend for cloud sync; mirror SQLite schema.
- **Avalonia GUI**: ReactiveUI for MVVM; share models from `MyLittleRangeBook/`.

## References
- Entry point: `MyLittleRangeBook.CLI/Program.cs`
- DI setup: `MyLittleRangeBook.Sqlite/SqliteHelperExtensions.cs`
- Service example: `MyLittleRangeBook.Sqlite/SqliteSimpleRangeEventService.cs`
- Model: `MyLittleRangeBook/Models/SimpleRangeEvent.cs`
- Config: `MyLittleRangeBook/Config/ConfigurationExtensions.cs`
