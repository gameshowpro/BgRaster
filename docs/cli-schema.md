# CLI Schema Reference

The BgRaster command-line interface is a thin overlay on top of the TOML configuration. Every CLI option overrides exactly one global TOML key; per-output and per-slice overrides remain TOML-only. Unspecified options preserve the TOML / default value.

## Synopsis

```
BgRaster.exe [--config <path>] [--<section>-<key> <value>]...
```

There are no sub-commands. There is no positional argument.

## Resolution order

1. Built-in defaults (see [TOML schema](toml-schema.md))
2. Values from `config.toml` (resolved via `--config`, or if omitted searched in this order: `<exe-dir>\config.toml`, `%ProgramData%\BgInfo\config.toml`, `%LocalAppData%\BgInfo\config.toml`, `%AppData%\BgInfo\config.toml`)
3. CLI overrides

CLI overrides accept either scalar values or TOML array literals, depending on the option type. Example: `--background-color "#000"` becomes `color = ["#000"]`, `--text-size "[\"3vh\",\"2vh\",\"4vh\"]"` preserves all elements, and `--logo-opacity "[0.5,1.0]"` sets numeric opacity values.

## Options

This table is generated from schema metadata in `docs/schemas/bgraster-config.schema.json` and validated by tests.

<!-- BEGIN:CLI_OPTIONS_TABLE -->
--8<-- "generated/cli-schema.md"
<!-- END:CLI_OPTIONS_TABLE -->

For unit, color, and substitution-token reference see the [TOML schema](toml-schema.md).

## Exit codes

| Code | Meaning |
|---|---|
| `0` | Success (or early exit because nothing changed). |
| `1` | Wallpaper assignment failed for at least one monitor. |

## Standard output

BgRaster emits machine-readable status lines as it runs. Every line is prefixed `# bg-raster: status=...` and is safe to grep. See [Status values](#status-values) below and the [troubleshooting guide](troubleshooting.md).

### Status values

| Status | Emitted by | Meaning |
|---|---|---|
| `output-rendered` | each successful render | A PNG was written for an output. |
| `output-not-found` | match phase | A configured target did not match any discovered output. |
| `duplicate-output-ignored` | match phase | A configured target matched an output already claimed by an earlier `[[output]]` entry. |
| `output-discovered` | end of run | Hardware output was found but left untouched because `outputs-skip-unspecified=true`. |
| `slice-rendered` | each successful slice | A slice rect was rendered. |
| `slice-out-of-bounds` | slice geometry check | A slice rect exceeded the output bounds; rendering skipped. |
| `wallpaper-assignment-failed` | wallpaper phase | `IDesktopWallpaper::SetWallpaper` returned a failure HRESULT. |
| `run-skipped-unchanged` | early-exit check | Version, settings hash, and hardware fingerprint all match the previous run. |
| `run-complete` | end of run | Normal completion. |

Status values also appear as comments above the corresponding blocks in `lastRun.toml`.

## Examples

```pwsh
# Use a specific config file
BgRaster.exe --config "C:\AV\bgraster\studio-a.toml"

# Dry run to inspect what would be rendered
BgRaster.exe --no-assignment true --render-output "C:\temp\bgraster-out"

# Force a color wash without editing config
BgRaster.exe --background-color "#101820" --grid-size 0

# Quick coordinate grid for projector calibration
BgRaster.exe --grid-size 50px --grid-coordinates true --background-color "#000000"
```
