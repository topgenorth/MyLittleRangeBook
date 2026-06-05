# AGENTS: How to be productive in the MyLittleRangeBook repo

This file gives actionable, repository-specific guidance for AI coding agents working on MyLittleRangeBook. It highlights the architecture, common patterns, important files, and precise developer workflows an agent should follow to make safe, correct changes.

1) Big picture (what to read first)
- Read `.github/copilot-instructions.md` (it contains most repo knowledge: build, DB, DI patterns). See `MyLittleRangeBook.CLI/Program.cs` to understand host/DI setup.
- Key directories: `MyLittleRangeBook/` (core models, interfaces), `MyLittleRangeBook.CLI/` (entrypoint), `MyLittleRangeBook.FIT/` (FIT parsing). Ignore MyLittleRangeBook.GUI for now.

2) Architecture & why
- Separation by interface: services and repositories are defined in core and implemented per-database in provider projects. Agents must change interfaces in `MyLittleRangeBook/Services/` and then update providers, not vice-versa.
- Key design choices:
  - All DB work is async and returns FluentResults (`Result<T>`). Methods are `Task<Result<T>>`.
  - DB implementations use Dapper with handwritten SQL (no LINQ). Dapper AOT codegen is used; stale build artifacts break builds.
  - DI uses keyed registrations for multi-database support (see `SqliteHelperExtensions.AddMyLittleRangeBookSqlite()`).

3) Concrete patterns to follow (examples)
- Error handling: return `Result<T>` instead of throwing. Example: `public async Task<Result<bool>> DeleteAsync(...)` in `MyLittleRangeBook.Sqlite`.
- ID semantics: `Id` = ULID (string) for cross-system identity; `RowId` = nullable long used for SQLite ROWID lookups. See `MyLittleRangeBook/Models/SimpleRangeEvent.cs`.
- Parameterized SQL only. Example snippet from repo:
  ```csharp
  const string DeleteSql = "DELETE FROM SimpleRangeEvents WHERE Id = @Id;";
  cmd.Parameters.AddWithValue("@Id", evt.Id);
  ```
- When making SQL connections use the `ScopedSqliteConnection` from `ISqliteHelper.GetScopedSqliteConnection`.
- Custom SQL functions registered in SQLite provider: `nanoid()` (which will actually create a ULID as a string) and `utcnow()` (with will be a .NET `DateTimeOffset`) (see `SqliteHelperExtensions.AddFunctions()`).

4) Build / test / debug workflows (must-do steps)
- Always run `dotnet restore` from repository root before builds to avoid Dapper.AOT generator errors.
- If you see trimming / Dapper.AOT errors, run full purge-clean: `./src/mlrb/purge-clean.ps1` (Windows) or `./src/mlrb/purge-clean.sh` (Unix) or remove `bin/` and `obj/` and then `dotnet clean && dotnet restore && dotnet build`.
- Build examples:
  - `dotnet build` (whole solution)
  - `dotnet build src/mlrb/MyLittleRangeBook.CLI -c Release`
  - Publish single-file: `dotnet publish src/mlrb/MyLittleRangeBook.CLI -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained`
- Tests: run `dotnet test`. Use `--filter` to focus (e.g. `--filter "ClassName=DatabaseTypeHandlerTests"`). GUI tests are disabled in CI.

5) Common failure modes and remediation
- Dapper AOT / trimming: purge build artifacts (see above). Failure here is frequent after changing data layer.
- Missing DI registration: check `AddMyLittleRangeBookSqlite()` / `AddPostgresHelper()`; missing keyed registration leads to null services at runtime.
- Environment/Config: `MyLittleRangeBook/Config/ConfigurationExtensions.cs` determines file locations and environment behavior; verify `DefaultSqliteDatabaseName()` when debugging connection issues.

6) What to edit and how (agent checklist for a typical change)
- Read interface in `MyLittleRangeBook/Services/` before editing provider code.
- Modify core model or interface → update both `MyLittleRangeBook.Sqlite/` and `MyLittleRangeBook.PgSQL/` implementations.
- Add SQL schema changes in `MyLittleRangeBook.Sqlite/Scripts/` and use DBUp conventions seen in that folder.
- Preserve async/fluent-results signatures and parameterized SQL.
- Run `dotnet restore && dotnet build` then `dotnet test` and, if touching DB code, run purge-clean when necessary.

7) Important files to inspect when working in the repo (short pointers)
- `.github/copilot-instructions.md` — authoritative repo-specific rules.
- `MyLittleRangeBook.CLI/Program.cs` — Host/DI setup and logging (how services are wired).
- `MyLittleRangeBook/Models/SimpleRangeEvent.cs` — ID/RowId/Created/Modified conventions.
- `src/mlrb/purge-clean.ps1` and `purge-clean.sh` — recommended cleanup scripts.

8) Permissions & external integrations
- Avoid hardcoding secrets. Use environment variables or `appsettings.*` as the repo demonstrates.

9) Safety rules for agents
- Do not change concrete DB registrations without updating consumer code and tests.
- Preserve FluentResults error-return style; do not convert to exceptions.
- Keep changes small and run unit tests locally. If you introduce SQL schema changes, also add DBUp scripts and migration tests.

---
When in doubt, open the three files listed under "Important files to inspect" and run a full `dotnet restore && dotnet build && dotnet test` before opening a PR.
