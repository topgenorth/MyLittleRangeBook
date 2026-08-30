---
name: add-cli-command
description: Add a new ConsoleAppFramework CLI command for MyLittleRangeBook using async patterns, Spectre.Console output, and DI.
---

# Purpose
Use this skill when asked to add or modify a CLI command in MyLittleRangeBook.

# Project context
- Solution contains one app: CLI.
- Prefer implementing features in CLI first.
- Target framework is .NET 10.
- CLI uses ConsoleAppFramework and Spectre.Console.
- Spectre.Console commands should be in their own class.
- Use the Result pattern (via `FluentResults`) for service/logic methods.
- Prefer async methods everywhere possible.
- The Fisher framework (from the Critter Stack) is used for data storage.

# Instructions
1. The CLI project is MyLittleRangeBook.CLI.
2. Core logic exists in only the MyLittleRangebook project.
3. Command classes should be sealed.
4. The commands will be in directories that are named after the document.  For example `Firearms`, `RangeEvents`, and cartridges.
5. Follow the existing namespace, DI, and command registration patterns.
6. Create async command handlers returning `Task<int>`.
7. Keep Spectre.Console command logic thin; move business logic into services.
8. Use Spectre.Console (via `CliDisplay`) for user-facing output.
9. Add or update tests where the repo pattern suggests.
10. Keep changes minimal and consistent with existing naming.
11. Spectre.Console commands return an integer value for any issues.  Update the file ReturnCodes.cs with a descriptive constant for the integer.

# Output expectations
- Show files to add/change.
- Explain assumptions briefly.
- Prefer compilable code over pseudocode.

# Examples
- Add command: `mlrb rangeevent add --firearm "Glock 19" --rounds 50 --range "Bullseye"`
- Add command: `mlrb assets import --file "C:\Users\tom\Code\MyLittleRangeBook\src\mlrb\sample-fit\12-31-2025_12-19-19.fit"`
- Add command: `mlrb rangeevent list`