---
name: mlrb-feature-implementation
description: Use this skill when implementing features in MyLittleRangeBook under src/mlrb, especially CLI commands, Garmin FIT import flows, SQLite or PostgreSQL data access with Dapper, shared domain models, and Avalonia GUI features. Prefer async methods, follow existing command and repository patterns, and preserve cross-platform .NET 10 compatibility.
---

# MyLittleRangeBook Feature Implementation

Use this skill when working in the MyLittleRangeBook solution located at `src/mlrb`.

## Purpose
This solution is a .NET 10 CLI Desktop application:

- CLI app using ConsoleAppFramework and Spectre.Console.
- Shared/domain libraries for application logic, FIT parsing, SQLite, PostgreSQL, IO, and tests.

The main purpose of the application is to act as a marksman log book:
- Capture data about trips to the range.
- Import and process for Garmin Xero FIT files.
- Import and process CSV files from the Garmin ShotView app.
- Import and process pictures of target.
- Store and query firearms, range events, and FIT-derived data.
- Use Event Sourcing for core aggregates (e.g., Firearms).

## Project priorities

Follow these priorities in order:

1. Prefer the CLI path first when implementing new functionality.
2. Ensure that AOT will not break anything.
3. Ignore the MyLittleRangeBook.GUI and MyLittleRangeBook.GUI.Tests folders/project.
4. Prefer async methods where ever practical and supported by the codebase.
5. Reuse existing abstractions and patterns before introducing new ones.
6. Keep the solution cross-platform for Windows, Ubuntu Linux, and macOS.
7. Preserve single-file, self-contained app compatibility.
8. Use Dapper for data access, usually via `DapperCommandContext`.
9. Use the Result pattern (via `FluentResults`) for all service and repository operations.
10. SQLite is the primary database. Ignore PostgreSQL/Supabase unless explicitly requested.
11. Directories that start with an _ are for file organization only and are not to be used as a namespace provider. For example, files in the directory MyLittleRangeBook.CLI/_Commands/_Firearms belong in the namespace MyLittleRangeBook, and not MyLittleRangeBook/Commands/Firearms.

## Solution structure

Important projects under `src/mlrb` include:

- `MyLittleRangeBook.CLI` — primary feature surface right now.
- `MyLittleRangeBook.FIT` — FIT parsing and Garmin Xero-related domain logic.
- `MyLittleRangeBook` — shared models, configuration, service interfaces.
- `MyLittleRangeBook.Tests` and `MyLittleRangeBook.GUI.Tests` — tests.

## Implementation rules

### General

- Match existing naming, file placement, and namespace conventions.
- Use `MlrbId` for all entity and aggregate identifiers.
- Use `Result<T>` or `Result` (from `FluentResults`) for return types in services and repositories.
- Do not introduce unnecessary architectural layers.
- Favor extending existing services, repositories, printers, and command groups.
- Keep methods focused and composable.
- Use cancellation-aware async APIs where practical.
- Avoid synchronous database or file access when an async option is reasonable.
- Do not rewrite unrelated code during feature work.

### CLI rules

When implementing CLI features:

- Follow patterns already present in:
  - `MyLittleRangeBook.CLI`
  - `Program.cs`
  - console display and printer helpers.
- Prefer adding a new command class or extending an existing command group rather than placing logic directly in `Program.cs`.
- Keep command handlers thin.
- Put business logic in services/helpers, not directly in command methods.
- Use Spectre.Console output conventions already present in the repo.
- Reuse existing printers such as range event, firearms, and table printers when possible.
- Return meaningful success/failure results instead of throwing for expected user errors.

### Data access rules

When implementing persistence:

- Prefer existing service/repository interfaces first.
- Use Dapper via `DapperCommandContext`.
- Prefer async methods such as:
  - `OpenAsync`
  - `ExecuteAsync`
  - `QueryAsync`
  - `QueryFirstOrDefaultAsync`
- Parameterize SQL; never build SQL by concatenating untrusted input.
- Keep SQL close to the repository/service that owns it.
- For SQLite changes, check whether a migration script is needed in:
  - `MyLittleRangeBook/Persistence/Sqlite/Scripts/`
- If adding a feature that should exist in both SQLite and PostgreSQL, keep the shared contract in a common project and implement provider-specific details in each database project (though SQLite is the current priority).

### Event Sourcing rules

When working with Event Sourced aggregates (e.g., `FirearmAggregate`):

- Inherit from `Aggregate`.
- Use `SqliteAggregateRepository<TAggregate>` for persistence.
- Define domain events as nested classes within the Aggregate class.
- Implement `Apply` methods for each event to update internal state.
- Use Projectors (`IProjector`) to update read models in the database.
- Register projectors with a unique `DI_KEY`.

### FIT import rules

For Garmin Xero/FIT-related work:

- Reuse types and parsing flow in `MyLittleRangeBook.FIT`.
- Prefer extending parser/domain types over duplicating FIT parsing logic in CLI or GUI projects.
- Keep file validation and parsing errors explicit and user-friendly.
- Treat FIT import as a pipeline:
  1. Validate input path.
  2. Parse FIT content.
  3. Map to domain models.
  4. Persist through the appropriate service/repository.
  5. Present concise results to the user.

### GUI rules

When implementing Avalonia features:

- Follow the existing View / ViewModel structure already present in `MyLittleRangeBook.GUI`.
- Keep UI logic in ViewModels, not code-behind, unless it is truly view-specific.
- Reuse existing storage/services/messages patterns.
- Keep GUI work aligned with shared business/domain services instead of creating GUI-only domain behavior.

## Files and patterns to inspect first

Before making changes, inspect relevant existing files and imitate their style.

### CLI command patterns

Look at examples such as:

- `MyLittleRangeBook.CLI/RangeEvents/SimpleRangeEventCommandAddToSqlite.cs`
- `MyLittleRangeBook.CLI/Commands/GarminFitFileCommands.cs`
- `MyLittleRangeBook.CLI/Commands/ImportFitFileToSqliteCommand.cs`
- `MyLittleRangeBook.CLI/Console/*.cs`

### SQLite patterns

Look at:

- `MyLittleRangeBook/Persistence/Sqlite/SqliteHelper.cs`
- `MyLittleRangeBook/RangeEvents/SqliteSimpleRangeEventService.cs`
- `MyLittleRangeBook/MlrbAssets/Handlers/InsertAssetFileSqliteHandler.cs`
- `MyLittleRangeBook/Persistence/Sqlite/Scripts/*.sql`

### Event Sourcing patterns

Look at:

- `MyLittleRangeBook/EventSourcing/Aggregate.cs`
- `MyLittleRangeBook/EventSourcing/SqliteAggregateRepository.cs`
- `MyLittleRangeBook/Firearms/FirearmAggregate.cs`
- `MyLittleRangeBook/Firearms/SqliteFirearmAggregateRepository.cs`
- `MyLittleRangeBook/Firearms/FirearmProjector.cs`
- `MyLittleRangeBook/MlrbAssets/MlrbAssetAggregate.cs`

### FIT patterns

Look at:

- `MyLittleRangeBook.FIT/XeroShotSessionParser.cs`
- `MyLittleRangeBook.FIT/XeroFitFile.cs`
- `MyLittleRangeBook.FIT/Model/*.cs`

### GUI patterns

Look at:

- `MyLittleRangeBook.GUI/**/ViewModels/*.cs`
- `MyLittleRangeBook.GUI/**/Views/*.axaml`
- `MyLittleRangeBook.GUI/Services/*.cs`

## Feature workflow

For any requested feature, follow this sequence:

1. Identify the user-facing entry point:
   - CLI command,
   - GUI screen/action,
   - shared service,
   - import pipeline,
   - database layer.

2. Find an existing nearby implementation and mirror its structure.

3. Update or add shared abstractions first when needed:
   - interfaces,
   - models,
   - result types.

4. Implement database/service logic next, preferring async methods.

5. Wire the feature into the CLI or GUI surface.

6. Add or update tests in the most relevant test project.

7. Verify build impact is limited to the feature area.

## Output expectations

When completing a feature:

- Summarize which files were changed.
- Explain any schema or migration changes.
- Note whether SQLite, PostgreSQL, CLI, GUI, and tests were updated.
- Call out any follow-up work if one provider or UI surface is intentionally deferred.

## Guardrails

- Do not replace Dapper with another ORM.
- Do not introduce blocking `.Result` or `.Wait()` calls unless absolutely unavoidable.
- Do not add platform-specific behavior unless required and clearly isolated.
- Do not add large new dependencies unless the feature clearly demands them.
- Do not move feature logic into the GUI if it belongs in shared or CLI-accessible services.
- Do not bypass existing helper/services patterns just to implement quickly.

## When this skill is a strong match

This skill is especially relevant for prompts like:

- “Add a new CLI command to import or list data.”
- “Implement Garmin Xero FIT import support.”
- “Add SQLite or PostgreSQL persistence for a new entity.”
- “Add a new Avalonia screen for an existing domain workflow.”
- “Refactor this feature to follow async Dapper patterns.”
- “Add tests for range-event or FIT-related workflows.”

## Definition of done

A feature is considered complete when:

- The implementation follows existing repo patterns.
- Async methods are used where appropriate.
- Data access uses Dapper safely.
- Cross-platform assumptions are preserved.
- The correct project boundaries are respected.
- Tests are added or updated where practical.
- User-facing output is clear and consistent.