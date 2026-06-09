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
2. `%ProgramData%\BgInfo\config.toml`
3. `%LocalAppData%\BgInfo\config.toml`
4. `%AppData%\BgInfo\config.toml`

If none exist, use built-in defaults.

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
