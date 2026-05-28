# BgRaster

A .NET 10 Native AOT console application that generates per-output diagnostic backgrounds for Windows display configurations and assigns them as desktop wallpaper. Designed for AV technicians, video engineers, and broadcast control rooms that need to quickly identify outputs, verify resolution, check colour, validate scaling, and detect signal-path issues.

## Highlights

- **One PNG per video output** — each Windows monitor receives a wallpaper sized to its native resolution, anchored to its desktop position. No stretched panoramas.
- **Composable diagnostic layers** — solid background, alternating-pixel pattern, regular grid (with optional WCAG-aware coordinate labels), border, centred circle, crosshair, logo, and substitution-aware text.
- **Per-output and per-slice overrides** — global defaults cascade to outputs, then to optional rectangular slices within an output (useful for video walls and processed feeds).
- **Native AOT** — single-file `.exe`, no .NET runtime dependency, fast startup.
- **Idempotent** — early-exits when configuration, version, and hardware fingerprint all match the previous run.
- **Diagnostic `lastRun.toml`** — every run writes the effective configuration plus per-output / per-slice status comments, then verifies the file round-trips before atomically replacing the previous one.

## Supported environments

| Component | Requirement |
|---|---|
| OS | Windows 10 1809+ or Windows 11 |
| Architecture | x64 |
| Privileges | Administrator (required by `IDesktopWallpaper` for the SYSTEM session) |
| DPI | Per-Monitor v2 aware |

Linux and macOS are not supported — the project is intentionally Windows-specific.

## Quick start

### Install
Download the latest `BgRaster.exe` from the Releases page (or build it yourself, see below) and place it anywhere on disk.

### Build from source
```pwsh
git clone https://github.com/gameshowpro/GameshowPro.git
cd GameshowPro/BgRaster
dotnet publish src/BgRaster.csproj -c Release -r win-x64 /p:PublishAot=true
```
The published binary lands in `src/bin/Release/net10.0/win-x64/publish/BgRaster.exe`.

### First run (dry run, no wallpaper changes)
```pwsh
.\BgRaster.exe --no-assignment
```
This writes one PNG per discovered output to `%TEMP%\BgRaster\` and a `lastRun.dry.toml` describing what it did.

### Apply wallpaper
```pwsh
.\BgRaster.exe
```
Run elevated. Reads `config.toml` next to the executable (if present), generates one PNG per output, assigns each as its monitor's wallpaper, recycles stale files from previous runs, and writes `lastRun.toml`.

### Override on the command line
```pwsh
.\BgRaster.exe --grid-size 50px --grid-coordinates true --background-color "#202020"
```

## Sample configurations

**Identification overlay** — show machine name, output index, and resolution on each screen:
```toml
[text]
title = ["${MachineName}"]
subtitle = ["Output ${IndexPlusOne} - ${Width}x${Height}"]
size = ["3vh"]

[background]
color = ["#101820"]

[grid]
size = ["100px"]
coordinates = [true]
```

**Per-output colour wash** — cycle red / green / blue across three outputs:
```toml
[background]
color = ["#FF0000", "#00FF00", "#0000FF"]
```

**Slice a 4K wall into four HD quadrants**:
```toml
[[output]]
target = 0

[[output.slice]]
x = "0"; y = "0"; width = "50vw"; height = "50vh"
[[output.slice]]
x = "50vw"; y = "0"; width = "50vw"; height = "50vh"
[[output.slice]]
x = "0"; y = "50vh"; width = "50vw"; height = "50vh"
[[output.slice]]
x = "50vw"; y = "50vh"; width = "50vw"; height = "50vh"
```

## Documentation

- [TOML schema reference](docs/toml-schema.md) — every section, key, type, default, and unit.
- [CLI schema reference](docs/cli-schema.md) — every command-line option mapped to its TOML equivalent.
- [Architecture overview](docs/architecture.md) — runtime pipeline, modules, and AOT decisions.
- [Troubleshooting guide](docs/troubleshooting.md) — DPI, output matching, lastRun diagnostics, recycle behaviour, early-exit.
- [Deferred tasks](docs/deferred-tasks.md) — known limitations and future work.

## License

See repository root.
