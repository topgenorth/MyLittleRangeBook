---
name: dapper-database-service-pattern
description: Add or refactor a service in MyLittleRangeBook using Dapper, async-first repository and service patterns, and compatibility with SQLite and PostgreSQL.
***

# Purpose
Use this skill when implementing or modifying database access in MyLittleRangeBook.

# Project context
- The solution targets .NET 10.
- Persistence uses Dapper ORM.
- Supported databases are SQLite and PostgreSQL.
- Prefer async methods for all database operations where provider support exists.
- The code should support both CLI and Avalonia application flows.

# When to use
Use this skill when the task involves:
- A single table within the database.
- Refactoring ad hoc SQL access into a repository or service.
- Creating queries, inserts, updates, deletes, or upserts.
- Supporting both SQLite and PostgreSQL behavior.
- Adding persistence for trips, sessions, firearms, ammo, imports, or related marksmanship data.

Do not use this skill for UI-only tasks or command wiring unless persistence design is the main concern.

# Service workflow
1. Inspect the existing data access conventions before adding new code.
2. Prefer a small service interface when abstraction already exists or clearly improves testability. T
3. The interface should have Delete, Upsert, Get, and GetAll methods. The methods should be named in a way that clearly indicates their purpose, such as DeleteTrip, UpsertSession, GetFirearm, GetAllAmmo, etc.
3. The service should accept a connection as a parameter, it should not manage its own connection lifecycle.
4. Use async Dapper APIs when available.
5. Keep SQL explicit and readable; prefer parameterized queries only.
6. Separate SQL by purpose: lookup, insert, update, delete, reporting.
7. Return a FluentResult Result<T> object that will hold domain-oriented models, focused DTOs, or named tuples rather than leaking raw record shapes everywhere.
8. Handle transactions explicitly for multi-step writes.
9. Ensure SQL works for both SQLite and PostgreSQL, or isolate dialect differences cleanly.
10. Add narrow tests around mapping, SQL behavior, or repository semantics where practical.

# SQL rules
- Always parameterize inputs.
- Avoid string-concatenated SQL.
- Prefer simple, composable SQL over overly generic repository helpers.
- Be explicit about ordering when returning lists.
- Use UTC-aware timestamp storage and mapping.
- For inserts that need generated keys, handle SQLite and PostgreSQL differences deliberately.
- For paging or reporting queries, keep performance in mind and select only needed columns.

# Cross-database guidance
- Do not assume identical SQL syntax between SQLite and PostgreSQL.
- Isolate dialect-specific details when necessary, such as generated key retrieval, boolean handling, date functions, or upsert syntax.
- If one SQL statement cannot reasonably support both providers, separate the provider-specific query paths behind a small abstraction.
- Keep schema expectations conservative so local SQLite development remains simple.

# Design rules
- Repositories should focus on persistence, not business orchestration.
- Services should compose repositories when a use case spans multiple tables or steps.
- Prefer immutable or clearly initialized models for mapped results.
- Make cancellation support available if the repository already uses it or if new async APIs are being introduced broadly.
- Keep schema changes minimal and aligned with real domain needs.

# Output rules
- Produce code that fits the repository's existing architecture.
- Show changed files.
- Keep the implementation incremental rather than speculative.
- Briefly note any schema migration implications.

# Examples
- Add `ITripRepository` and a Dapper-backed implementation.
- Store imported FIT batches and related shot records.
- Add a PostgreSQL/SQLite-safe upsert for ammo lot metadata.
- Refactor direct SQL from a command into a repository plus service.