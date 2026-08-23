# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

The canonical, up-to-date guidance for AI agents lives in [AGENTS.md](./AGENTS.md).
Read it first — it covers the project layout, architecture, persistence patterns,
build/test workflows, and safety rules. This file intentionally defers to it to avoid
two sources of truth drifting apart.

## Quick reference

- Solution: `src/mlrb/MyLittleRangeBook.slnx` (run `dotnet` commands from `src/mlrb/`).
- Always `dotnet restore` before building; on Dapper.AOT/trimming errors run
  `./purge-clean.ps1` / `./purge-clean.sh` from `src/mlrb/`.
- SQLite only; persistence lives inside the core project under
  `MyLittleRangeBook/Persistence/`. All DB work is async and returns FluentResults.

See [AGENTS.md](./AGENTS.md) for everything else.
