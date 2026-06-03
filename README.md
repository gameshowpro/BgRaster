# BgRaster

## Inspiration
BgRaster is inspired by the classic Sysinternals utility [BgInfo](https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo). It was executed perfectly in 2000, but its structure and features feel dated today. In particular, it's focussed on the needs of an IT administrator from that era. It's missing many features that would be valuable to a creative professional in the 2020s.

## Summary
BgRaster is a fast console application that uses modern frameworks and APIs to generate and assign wallpapers for any displays it finds. It has many [features](/docs/features.md) targetted at creative professionals like AV technicians, video engineers, and real time graphics specialists. It has a rich set of command-line options, but its full power lies in its [TOML](https://en.wikipedia.org/wiki/TOML) configuration files.

## Highlights

- **One PNG per video output** — each Windows monitor receives a wallpaper sized to its native resolution, anchored to its desktop position.
- **Slices** — outputs have one slice by default, but can be split into any number, each with different settings.
- **Idempotent** — early-exits when configuration, version, and hardware fingerprint all match the previous run.
- **Portable** — single-file `.exe` without runtime dependencies, built for speed. 

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
subtitle = ["Output ${OutputIndexPlusOne} - ${Width}x${Height}"]
size = ["3vh"]

[background]
color = ["#101820"]

[grid]
size = ["100px"]
coordinates = [true]
```

**Per-output color wash** — cycle red / green / blue across three outputs:
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
