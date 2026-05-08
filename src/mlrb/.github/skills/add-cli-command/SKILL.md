---
name: add-cli-command
description: Add a new ConsoleAppFramework CLI command for MyLittleRangeBook using async patterns, Spectre.Console output, DI, and repository-based persistence.
---

# Purpose
Use this skill when asked to add or modify a CLI command in MyLittleRangeBook.

# Project context
- Solution contains two apps: Avalonia GUI and CLI.
- Prefer implementing features in CLI first.
- Target framework is .NET 10.
- CLI uses ConsoleAppFramework and Spectre.Console.
- Prefer async methods everywhere possible.
- Data access uses Dapper.
- Databases supported: SQLite and PostgreSQL
- Domain includes range trips, Garmin Xero FIT files, shot/session logs, firearms, ammo, and related marksmanship records.

# Instructions
1. Find the CLI project and existing command organization.
2. Follow the existing namespace, DI, and command registration patterns.
3. Create async command handlers returning `Task` or `Task<int>`.
4. Keep command logic thin; move business logic into services.
5. Use repository abstractions around Dapper access.
6. Use Spectre.Console for user-facing output.
7. When persistence changes are needed, support both SQLite and PostgreSQL.
8. Add or update tests where the repo pattern suggests.
9. Keep changes minimal and consistent with existing naming.

# Output expectations
- Show files to add/change.
- Explain assumptions briefly.
- Prefer compilable code over pseudocode.

# Examples
- Add command: `mlrb rangetrip add`
- Add command: `mlrb fit import sqlite --fit-file "C:\Users\tom\Code\MyLittleRangeBook\src\mlrb\sample-fit\12-31-2025_12-19-19.fit"`
- Add command: `mlrb rangetrip list`