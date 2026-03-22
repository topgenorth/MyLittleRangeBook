#!/bin/bash
# install-go.sh — install or update Go on Linux Mint 22.3
# Based on Ubuntu 24.04; installs latest stable Go from go.dev

set -euo pipefail

readonly INSTALL_DIR="/usr/local/go"
readonly GO_DOWNLOAD_URL="https://go.dev/dl"
readonly ARCH="amd64"
readonly OS="linux"

# choose shell profile
if [[ "${SHELL##*/}" == "zsh" ]]; then
  PROFILE_FILE="$HOME/.zshrc"
elif [[ "${SHELL##*/}" == "bash" ]]; then
  PROFILE_FILE="$HOME/.bashrc"
else
  echo "Unsupported shell (not bash/zsh); PATH will NOT be auto‑set." >&2
  PROFILE_FILE=""
fi

# detect current Go version from $PATH
detect_current_version() {
  if command -v go >/dev/null 2>&1; then
    go version | awk '{print $3}'
  else
    echo "none"
  fi
}

# fetch latest Go version tag from the downloads page
fetch_latest_version() {
  curl -s "$GO_DOWNLOAD_URL/?mode=json&include=all" | \
    jq -r '.[] | select(.version | startswith("go1.")) | select(.files[].filename | contains("'$OS'-'$ARCH'")) | .version' | \
    head -1 || {
      echo "Failed to fetch latest Go version from API." >&2
      exit 1
    }
}

# parse Go version into a clean tag (e.g., 1.25.4)
parse_version() {
  local v="$1"
  if [[ "$v" =~ ^go([0-9]+\.[0-9]+(\.[0-9]+)?[^ ]*?)$ ]]; then
    echo "${BASH_REMATCH[1]}"
  else
    echo "$v"
  fi
}

# build tarball name and URL
compose_tarball_info() {
  local version="$1"
  local suffix="linux-amd64.tar.gz"
  if [[ "$version" =~ ^go ]]; then
    TARBALL="go${version#"go"}.${suffix}"
  else
    TARBALL="go${version}.${suffix}"
  fi
  TAR_URL="$GO_DOWNLOAD_URL/$TARBALL"
}

# download and install Go tarball
install_go() {
  local version="$1"
  local tmpdir

  tmpdir="$(mktemp -d)"
  cd "$tmpdir"

  echo "Downloading Go $version..."
  if ! curl -LO "$TAR_URL"; then
    echo "Download failed: $TAR_URL" >&2
    exit 1
  fi

  # remove any existing install
  if [[ -d "$INSTALL_DIR" ]]; then
    echo "Removing existing Go at $INSTALL_DIR"
    sudo rm -r "$INSTALL_DIR"
  fi

  echo "Installing Go $version to $INSTALL_DIR"
  sudo tar -C /usr/local -xzf "$TARBALL"

  # clean up
  rm -rf "$tmpdir"

  echo "Go $version installed at $INSTALL_DIR"
}

# add Go to PATH in profile if not already there
add_to_path() {
  local path_entry="/usr/local/go/bin"

  if [[ -n "$PROFILE_FILE" && -f "$PROFILE_FILE" ]]; then
    if ! grep -q "$path_entry" "$PROFILE_FILE"; then
      echo "" >>"$PROFILE_FILE"
      echo "# Go (https://go.dev)" >>"$PROFILE_FILE"
      echo "export PATH=\"\$PATH:$path_entry\"" >>"$PROFILE_FILE"
      echo "Go PATH added to $PROFILE_FILE; reload with:"
      echo "  source $PROFILE_FILE"
    else
      echo "Go PATH already present in $PROFILE_FILE"
    fi
  fi
}

# main flow
main() {
  local current="$(detect_current_version)"
  if [[ "$current" != "none" ]]; then
    echo "Current Go version: $current"
  else
    echo "No Go currently installed."
  fi

  local latest
  latest="$(fetch_latest_version)"
  if [[ -z "$latest" ]]; then
    echo "Could not determine latest Go version." >&2
    exit 1
  fi

  echo "Latest stable Go: $latest"

  if [[ "$current" == "none" ]] || [[ "$(parse_version "$current")" != "$(parse_version "$latest")" ]]; then
    compose_tarball_info "$latest"
    install_go "$latest"
    add_to_path
  else
    echo "Go is already up to date ($latest)."
  fi
}

# MUST install jq for URL parsing
if ! command -v jq >/dev/null 2>&1; then
  echo "Installing jq (required for parsing Go release JSON)..."
  sudo apt update
  sudo apt install -y jq
fi

main


