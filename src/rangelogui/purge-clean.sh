#!/bin/sh
set -eu

# Run from the script directory.
SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
cd "$SCRIPT_DIR"

kill_processes() {
  if command -v pkill >/dev/null 2>&1; then
    for p in msbuild dotnet vbcscompiler devenv rider64; do
      pkill -f -x "$p" 2>/dev/null || true
    done
  fi
}

purge_build_dirs() {
  find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf -- {} + 2>/dev/null || true
}

kill_processes
purge_build_dirs

if command -v dotnet >/dev/null 2>&1; then
  dotnet nuget locals all --clear
  dotnet clean
  dotnet restore
  dotnet build
else
  printf '%s\n' "dotnet not found on PATH" >&2
  exit 1
fi