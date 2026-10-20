# Config File Logic

This document defines how BgRaster chooses, reads, and writes config files.

## Scope

Covers:

- explicit `--config` handling
- default config search behavior when `--config` is omitted
- unchanged-skip interaction with config seeding
- dry-run and force behaviors that affect config writes

## Key terms

- Explicit config: User passed `--config <path>`.
- Config exists at startup: The resolved explicit/default config path exists before the run starts.
- Unchanged run: Last-run comparison says configuration + version + hardware match.
- Continue after unchanged: Effective `[render].force` / `--render-force` is true.
- Seed config: Write a new config file containing effective values used for the run.

## Path resolution rules

1. All CLI and TOML paths are normalized when parsed.
2. Environment variables are expanded during normalization.
3. Relative file-system paths are resolved to absolute paths.
4. `pack://application:,,,/...` values are preserved as-is (not combined with base paths).

## Config source selection

### With explicit `--config`

- Use the explicit path after normalization.
- If the file exists: load it.
- If the file does not exist: start from built-in defaults, then apply CLI overlay.

### Without explicit `--config`

Search in this order and use the first existing file:

1. `config.toml` next to executable
2. `%LocalAppData%\BgRaster\config.toml`
3. `%AppData%\BgRaster\config.toml`
4. `%ProgramData%\BgRaster\config.toml`, only if administrators control it (see [Machine-wide config](#machine-wide-config))

If none exist, use built-in defaults.

These locations are only ever read. BgRaster never writes a config to them unless the same path was passed with `--config`. The first location exists for portable installs where a user placed `config.toml` next to the executable by hand. Under Chocolatey or winget it is the package's own install folder, which an upgrade or uninstall can replace.

## Machine-wide config

`%ProgramData%\BgRaster\config.toml` is shared by every user on the computer, so BgRaster uses it only if administrators control it. It is searched last, so a user's own config in an earlier location always wins.

### The rule

- The folder `%ProgramData%\BgRaster` and the file `config.toml` must each be owned by Administrators, SYSTEM or TrustedInstaller.
- Standard users must not be able to change, replace or delete the folder or the file. Read access, or the right to create new files in the folder, is fine.
- Neither the folder nor the file may be a junction or symbolic link.

### Why

Any standard user can create a folder under `%ProgramData%`, and owns whatever they create. Without this check, whoever created `%ProgramData%\BgRaster` first could decide what BgRaster does for every other user on the computer: which images it reads, where it writes output, and what text it draws.

### What happens when the config fails the rule

BgRaster ignores the file, logs a configuration warning that links to this section, and uses built-in defaults. `%ProgramData%` is the last location searched, so nothing else is read after it.

An explicit `--config` path and the other three locations are not checked. The user chose them or owns them.

### Set it up correctly

- Create the folder and the file from an administrative (elevated) shell. A non-elevated shell makes the current user the owner, even if that user is an administrator, and the config then fails the rule.
- Alternatively, install with Chocolatey. The package creates `%ProgramData%\BgRaster` with write access for administrators only. If the folder already exists and fails the rule, the package leaves it alone and prints a warning.
- The winget package and the zip do not create the folder.

The check assumes `%ProgramData%` keeps its default permissions, under which standard users cannot delete or rename folders they did not create.

## Decision table: render/assignment skip vs config write

`U` = unchanged run, `F` = continue after unchanged, `E` = explicit `--config` provided, `X` = explicit config exists at startup, `D` = dry-run

| Case | U | F | E | X | D | Render/Assign? | Seed explicit config? |
|---|---|---|---|---|---|---|---|
| 1 | 1 | 0 | 0 | - | 0/1 | No | No |
| 2 | 1 | 0 | 1 | 1 | 0/1 | No | No |
| 3 | 1 | 0 | 1 | 0 | 0 | No | Yes |
| 4 | 1 | 0 | 1 | 0 | 1 | No | No |
| 5 | 1 | 1 | 0/1 | 0/1 | 0/1 | Yes | Not on skip path |
| 6 | 0 | 0/1 | 0 | - | 0/1 | Yes | No |
| 7 | 0 | 0/1 | 1 | 1 | 0/1 | Yes | No |
| 8 | 0 | 0/1 | 1 | 0 | 0 | Yes | Yes (after successful execution) |
| 9 | 0 | 0/1 | 1 | 0 | 1 | Yes (dry render only) | No |

Notes:

- Case 3 is the critical clarified behavior: unchanged still skips render/assignment, but missing explicit config is written.
- For non-unchanged runs, seeding occurs only after successful execution and only when explicit config was missing at startup and not dry-run.

## Pseudocode

```text
resolve config path
check configExistsAtStartup
load config if exists else defaults
apply CLI overlay

compute unchanged
if unchanged:
  if !continueAfterUnchanged:
    if explicitConfig && !configExistsAtStartup && !dryRun:
      seed explicit config from effective options
    return success
  else:
    continue normal render flow

run render/assignment flow
if explicitConfig && !configExistsAtStartup && !dryRun:
  seed explicit config from effective options
return success/failure
```

## Why this behavior exists

- Keeps idempotent skip semantics for render/assignment.
- Still guarantees first-time explicit config paths become materialized templates.
- Avoids writing config files for implicit/default path resolution unless explicitly requested.
