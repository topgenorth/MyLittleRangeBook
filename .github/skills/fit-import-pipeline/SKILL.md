---
name: fit-import-pipeline
description: Implement or modify Garmin Xero FIT import workflows in MyLittleRangeBook, including parsing, validation, mapping to domain models, and async persistence.
---

# Purpose
Use this skill when work involves importing Garmin Xero FIT data into MyLittleRangeBook.

# Project context
- MyLittleRangeBook is a .NET 10 solution with a CLI and an Avalonia GUI.
- The CLI is the preferred place to add functionality first.
- Persistence uses Dapper with SQLite and PostgreSQL support.
- Prefer async methods for IO, parsing pipelines, and persistence.
- The application domain centers on marksmanship records such as trips, sessions, firearms, ammo, shot strings, and imported device data.

# When to use
Use this skill when the user asks to:
- Import FIT files from a Garmin Xero device.
- Add parsing support for new FIT message types.
- Map device records to domain entities.
- Validate or deduplicate incoming FIT data.
- Build CLI commands or services around FIT ingestion.

# Import workflow
1. Identify where import commands, services, or pipelines belong in the existing solution structure.
2. Locate the FIT parsing library already in use; if none exists, choose a .NET-compatible approach that fits the repository's patterns.
3. Treat the FIT file as external device input and validate it before domain mapping.
4. Parse records into transport models first, not directly into database entities.
5. Normalize timestamps, units, enum-like values, and optional fields before persistence.
6. Map parsed records into domain concepts such as import batch, session, shot event, firearm context, or device metadata.
7. Make the import idempotent when possible by detecting duplicates using source file hashes or event timestamps.