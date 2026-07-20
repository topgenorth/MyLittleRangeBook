---
name: add-cli-command
description: Add a new ConsoleAppFramework CLI command for MyLittleRangeBook using async patterns, Spectre.Console output, DI, and repository-based persistence.
---

# Purpose
Use this skill when asked to add or modify a CLI command in MyLittleRangeBook.

# Project context
- Solution contains one app: CLI.
- Prefer implementing features in CLI first.
- Target framework is .NET 10.
- CLI uses ConsoleAppFramework and Spectre.Console.
- Commands  should inherit from `MlrbSqliteCommandBase`.
- Use the Result pattern (via `FluentResults`) for service/logic methods.
- Use `DapperCommandContext` for database access and transactions.
- Use `MlrbId` for all entity and aggregate IDs.
- Prefer async methods everywhere possible.
- Data access uses Dapper.
- Databases supported: SQLite only. Ignore anything Supabase or PostgreSQL.
- Domain includes range trips, Garmin Xero FIT files, shot/session logs, firearms, ammo, and related marksmanship records.

# Instructions
1. Find the CLI project and existing command organization.
2. Follow the existing namespace, DI, and command registration patterns.
3. Inherit from `MlrbSqliteCommandBase` for SQLite-based commands.
4. Create async command handlers returning `Task<int>`.
5. Keep command logic thin; move business logic into services.
6. Use `DapperCommandContext` for transactional operations.
7. Use repository abstractions around Dapper access.
8. Use `MlrbId` for IDs.
9. Use Spectre.Console (via `CliDisplay`) for user-facing output.
10. When persistence changes are needed, support only SQLite.
11. Add or update tests where the repo pattern suggests.
12. Keep changes minimal and consistent with existing naming.

# Output expectations
- Show files to add/change.
- Explain assumptions briefly.
- Prefer compilable code over pseudocode.

# Examples
- Add command: `mlrb rangeevent add --firearm "Glock 19" --rounds 50 --range "Bullseye"`
- Add command: `mlrb assets import --file "C:\Users\tom\Code\MyLittleRangeBook\src\mlrb\sample-fit\12-31-2025_12-19-19.fit"`
- Add command: `mlrb rangeevent list`