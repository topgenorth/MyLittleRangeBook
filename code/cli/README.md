# Overview

This is the course code for the CLI stuff.

## Requirements

Go v1.17

## Building the app

Building the project is handled via [Mage](https://magefile.org/).  `mage build` will compile the app and put the
resulting binary at **./

```bash
mage -l
Targets:
  build          A build step that requires additional params, or platform specific steps for example
  clean          up after yourself
  install        A custom install step if you need your bin someplace other than go/bin
  installDeps    Manage your deps, or running package managers.
```

Build commands:

* **build** &mdash; Compiles the app to **./output/mlrb.exe**.
* **install** &mdash; Compiles the app to **../../bin/mlrb.exe**.