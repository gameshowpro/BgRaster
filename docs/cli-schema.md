# CLI Schema Reference

The BgRaster command-line interface is a thin overlay on top of the TOML configuration. Every CLI option overrides exactly one global TOML key; per-output and per-slice overrides remain TOML-only. Unspecified options preserve the TOML / default value.

## Synopsis

```
BgRaster.exe [--config <path>] [--<section>-<key> <value>]...
```

There are no sub-commands. There is no positional argument.

## Resolution order

1. Built-in defaults (see [TOML schema](toml-schema.md))
2. Values from `config.toml` (resolved via `--config` or `<exe-dir>\config.toml`)
3. CLI overrides

A CLI override sets the value as a single-element array (so `--background-color "#000"` becomes `color = ["#000"]`, applied to every output).

## Options

This table is generated from the CLI option catalog in code (`src/Configuration/CliOptionCatalog.cs`) and validated by tests.

<!-- BEGIN:CLI_OPTIONS_TABLE -->
| Option | Type | TOML equivalent | Description | Default resolution |
|---|---|---|---|---|
| `--config <path>` | `string` | `-` | Path to a TOML config file. | if omitted, uses config.toml next to the executable. |
| `--text-title <s>` | `string` | `[text].title` | Override title text. | if omitted, defers to config.toml [text].title; if missing there, uses ["${MachineName} ${Index}"]. |
| `--text-subtitle <s>` | `string` | `[text].subtitle` | Override subtitle text. | if omitted, defers to config.toml [text].subtitle; if missing there, uses ["${Width}x${Height}"]. |
| `--text-size <dim>` | `string` | `[text].size` | Font height. | if omitted, defers to config.toml [text].size; if missing there, uses ["1vh"]. |
| `--text-x <dim>` | `string` | `[text].x` | Anchor X. | if omitted, defers to config.toml [text].x; if missing there, uses ["75vw"]. |
| `--text-y <dim>` | `string` | `[text].y` | Anchor Y. | if omitted, defers to config.toml [text].y; if missing there, uses ["75vh"]. |
| `--background-color <colour>` | `string` | `[background].color` | Background fill colour. | if omitted, defers to config.toml [background].color; if missing there, uses ["#FF0000", "#00FF00", "#0000FF"]. |
| `--background-image <path>` | `string` | `[background].image` | Path to background bitmap. | if omitted, defers to config.toml [background].image; if missing there, uses [""] (disabled). |
| `--background-fit <mode>` | `string` | `[background].fit` | Background fit mode. | if omitted, defers to config.toml [background].fit; if missing there, uses ["CropToFill"]. |
| `--background-alternating <bool>` | `bool` | `[background].alternating` | Enable alternating-pixel pattern. | if omitted, defers to config.toml [background].alternating; if missing there, uses [false]. |
| `--background-border <bool>` | `bool` | `[background].border` | Enable viewport border. | if omitted, defers to config.toml [background].border; if missing there, uses [false]. |
| `--background-border-color <colour>` | `string` | `[background].border-color` | Border colour. | if omitted, defers to config.toml [background].border-color; if missing there, uses ["#FFFFFF"]. |
| `--grid-size <dim>` | `string` | `[grid].size` | Grid cell side length. | if omitted, defers to config.toml [grid].size; if missing there, uses ["100px"]. |
| `--grid-odd-color <colour>` | `string` | `[grid].odd-color` | Odd-cell colour. | if omitted, defers to config.toml [grid].odd-color; if missing there, uses ["#00000080"]. |
| `--grid-even-color <colour>` | `string` | `[grid].even-color` | Even-cell colour. | if omitted, defers to config.toml [grid].even-color; if missing there, uses ["transparent"]. |
| `--grid-stroke <dim>` | `string` | `[grid].stroke` | Cell stroke width. | if omitted, defers to config.toml [grid].stroke; if missing there, uses ["0"]. |
| `--grid-offset-x <dim>` | `string` | `[grid].offset-x` | Grid origin X. | if omitted, defers to config.toml [grid].offset-x; if missing there, uses ["0"]. |
| `--grid-offset-y <dim>` | `string` | `[grid].offset-y` | Grid origin Y. | if omitted, defers to config.toml [grid].offset-y; if missing there, uses ["0"]. |
| `--grid-coordinates <bool>` | `bool` | `[grid].coordinates` | Enable per-cell coordinate labels. | if omitted, defers to config.toml [grid].coordinates; if missing there, uses [false]. |
| `--circle-size <dim>` | `string` | `[circle].size` | Circle diameter. | if omitted, defers to config.toml [circle].size; if missing there, uses ["100vmin"]. |
| `--circle-color <colour>` | `string` | `[circle].color` | Circle colour. | if omitted, defers to config.toml [circle].color; if missing there, uses ["#ffffff40"]. |
| `--circle-stroke <dim>` | `string` | `[circle].stroke` | Circle stroke width. | if omitted, defers to config.toml [circle].stroke; if missing there, uses ["0"]. |
| `--crosshair-length <dim>` | `string` | `[crosshair].length` | Crosshair half-arm length. | if omitted, defers to config.toml [crosshair].length; if missing there, uses ["5vmin"]. |
| `--crosshair-color <colour>` | `string` | `[crosshair].color` | Crosshair colour. | if omitted, defers to config.toml [crosshair].color; if missing there, uses ["#ffffff80"]. |
| `--crosshair-stroke <dim>` | `string` | `[crosshair].stroke` | Crosshair stroke width. | if omitted, defers to config.toml [crosshair].stroke; if missing there, uses ["2px"]. |
| `--logo-source <path>` | `string` | `[logo].source` | Path to logo file (PNG/JPG/SVG). | if omitted, defers to config.toml [logo].source; if missing there, uses [""] (embedded fallback). |
| `--logo-x <dim>` | `string` | `[logo].x` | Logo rect left. | if omitted, defers to config.toml [logo].x; if missing there, uses ["75vw"]. |
| `--logo-y <dim>` | `string` | `[logo].y` | Logo rect top. | if omitted, defers to config.toml [logo].y; if missing there, uses ["25vh"]. |
| `--logo-width <dim>` | `string` | `[logo].width` | Logo rect width. | if omitted, defers to config.toml [logo].width; if missing there, uses ["15vw"]. |
| `--logo-height <dim>` | `string` | `[logo].height` | Logo rect height. | if omitted, defers to config.toml [logo].height; if missing there, uses ["15vh"]. |
| `--logo-opacity <0..1>` | `string` | `[logo].opacity` | Logo alpha multiplier. | if omitted, defers to config.toml [logo].opacity; if missing there, uses ["1"]. |
| `--no-assignment <bool>` | `bool` | `[render].no-assignment` | Generate PNGs without assigning wallpaper. | if omitted, defers to config.toml [render].no-assignment; if missing there, uses false. |
| `--outputs-skip-unspecified <bool>` | `bool` | `[render].outputs-skip-unspecified` | Skip discovered displays that have no explicit [[output]] target. | if omitted, defers to config.toml [render].outputs-skip-unspecified; if missing there, uses false. |
| `--render-output <path>` | `string` | `[render].output` | Output directory for generated PNGs. | if omitted, defers to config.toml [render].output; if missing there, defaults to %TEMP%/BgRaster. |
| `--render-force <bool>` | `bool` | `[render].force` | Continue rendering even after emitting run-skipped-unchanged. | if omitted, defers to config.toml [render].force; if missing there, uses false. |
| `--verbosity <level>` | `string` | `[render].verbosity` | Logging verbosity: quiet, normal, verbose. | if omitted, defers to config.toml [render].verbosity; if missing there, uses "normal". |
<!-- END:CLI_OPTIONS_TABLE -->

For unit, colour, and substitution-token reference see the [TOML schema](toml-schema.md).

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

# Force a colour wash without editing config
BgRaster.exe --background-color "#101820" --grid-size 0

# Quick coordinate grid for projector calibration
BgRaster.exe --grid-size 50px --grid-coordinates true --background-color "#000000"
```
