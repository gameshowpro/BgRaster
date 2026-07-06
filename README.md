# <img src="resources/BgRaster.svg" alt="BgRaster logo" width="32" height="32" style="vertical-align: top" /> BgRaster

## Inspiration
BgRaster is inspired by the classic Sysinternals utility [BgInfo](https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo). It was executed perfectly in 2000, but its structure and features feel dated today. In particular, it's focussed on the needs of an IT administrator from that era. It's missing many features that would be valuable to a creative professional in the 2020s.

## Summary
BgRaster is a fast console application that uses modern frameworks and APIs to generate and assign wallpapers for any displays it finds. It has many [features](https://bgraster.gameshow.pro/features/) targetted at creative professionals like AV technicians, video engineers, and real time graphics specialists. It has a rich set of command-line options, but its full power lies in its [TOML](https://bgraster.gameshow.pro/toml-schema/) configuration files.

## Highlights

- **One PNG per video output** - each Windows monitor receives a wallpaper sized to its native resolution, anchored to its desktop position.
- **Multiple layers** - text, network adapter info, labeled edges, custom logos (SVG), alignment grids, crosshairs, and solid backgrounds, each with independent positioning and styling.
- **Format strings** - `${tokens}` for dynamic substitution in text and network layers, with `<br>` line breaks and independent size/color cycling per rendered line.
- **Slices** - outputs have one slice by default, but can be split into any number, each with different settings.
- **Short-lived** - BgRaster runs until its work is done then exits. No lingering processes, no HTML rendering overhead, no OS hooks.
- **Idempotent** - early-exits when configuration, version, and hardware fingerprint all match the previous run.
- **Portable** - single-file `.exe` without runtime dependencies, built for speed via NativeAOT compilation.

## Sample output

![Slices sample](https://bgraster.gameshow.pro/generated/slices_0.png)

## Supported environments

| Component | Requirement |
|---|---|
| OS | Windows 10 1809+ or Windows 11 |
| Architecture | x64 |
| Privileges | Any user (IDesktopWallpaper COM interface) |
| DPI | Per-Monitor v2 aware |

Linux and macOS are not supported - the project is intentionally Windows-specific.

## When does the wallpaper get updated?
Like the original BgInfo, BgRaster runs gets its work done as fast as possible and shuts down. Depending on your use case, you might want to automate its execution with a login script, scheduled task or simply a desktop shortcut.

## Quick start

### Option 1 - Chocolatey
With [Chocolatey](https://chocolatey.org/) installed and the [default repository](https://community.chocolatey.org/) in your sources list, run:

```pwsh
choco install BgRaster -y
```

### Option 2 - Manual Install
Download the latest `BgRaster.zip` from the [Releases](https://github.com/gameshowpro/BgRaster/releases) page, unzip it, and place the extracted files anywhere on disk. `BgRaster.exe` and `libSkiaSharp.dll` need to stay in the same folder. If you want debugging symbols, download `BgRaster.pdb` separately.

### Option 3 - Build from source
```pwsh
git clone https://github.com/gameshowpro/GameshowPro.git
cd GameshowPro/BgRaster
dotnet publish src/BgRaster.csproj -c Release -r win-x64 /p:PublishAot=true
```
The published binary lands in `src/bin/Release/net10.0/win-x64/publish/BgRaster.exe`.

### First run, no arguments
```pwsh
.\BgRaster.exe
```
This writes one PNG per discovered output to `%TEMP%\BgRaster\` and a `lastRun.toml` describing what it did. The PNGs as assigned to the desktop of each output.

### Generate default config file
```pwsh
.\BgRaster.exe --config %programdata%\BgRaster\config.toml
```
If the file you specified doesn't exist, it will be created containing the defaults that were used. This is a great starting point for your own custom configuration.

The next time you run, it will load its config from this file rather than using defaults. If you don't specify a config file path, it will still find it if it exists in one of the default locations:
1. In the same folder as `BgRaster.exe`.
1. `%ProgramData%\BgRaster`
1. `%LocalAppData%\BgRaster`
1. `%AppData%\BgRaster`

## Sample configurations

**Identification overlay** - show machine name, output index, and resolution on each screen:
```toml
[text]
format = ["${MachineName} output ${OutputIndexPlusOne}"]
subtitle = ["${Width}x${Height}"]
size = ["3vh"]
x = ["85vw"]
y = ["85vh"]

[background]
color = ["#101820"]

[grid]
size = ["100px"]
coordinates = [true]
```

**Network adapter info** - display MAC, IP addresses, speed, and status per output:
```toml
[network]
render = true
adapter_format = "${Name}<br>${MacAddress}<br>${Speed} ${Status}<br>${IpAddresses}"
color = ["#000", "#a0a0a0", "#808080", "#FFFFFF"]
size = ["2vh", "2vh", "2vh", "3vh"]
x = "95vw"
y = "5vh"
anchor-x = "right"
text-align = "end"
```

`<br>` tokens create line breaks. Size and color arrays cycle independently per rendered line - first adapter name gets size[0]/color[0], first IP line gets size[1]/color[1], wrapping around.

**Per-output color wash** - cycle red / green / blue across three outputs:
```toml
[background]
color = ["#FF0000", "#00FF00", "#0000FF"]
```

**Slice a 4K wall into four HD quadrants**:
```toml
[[output]]
target = 0

[[output.slice]]
x = "0"
y = "0"
width = "50vw"
height = "50vh"

[[output.slice]]
x = "50vw"
y = "0"
width = "50vw"
height = "50vh"

[[output.slice]]
x = "0"
y = "50vh"
width = "50vw"
height = "50vh"

[[output.slice]]
x = "50vw"
y = "50vh"
width = "50vw"
height = "50vh"
```

**Labeled edges** - show desktop coordinates at screen edges for alignment (great for multi-projector blends):
```toml
[labeled-edges]
scope = "Desktop"
side = ["TL", "T", "TR", "R", "BR", "B", "BL", "L"]
text-size = ["10px"]
tail-length = ["10px"]
thickness = ["3px"]
```

## Documentation

[Documentation root](https://bgraster.gameshow.pro/)

- [Features](https://bgraster.gameshow.pro/features/) - A summary of visual features with images and config examples.
- [TOML schema reference](https://bgraster.gameshow.pro/toml-schema/) - every section, key, type, default, and unit.
- [CLI schema reference](https://bgraster.gameshow.pro/cli-schema/) - every command-line option mapped to its TOML equivalent.

## AI Disclosure
As a project started in 2026, yes, substantial parts of this application were built using AI tools. I could never have acheived the the application scope, automation, test framework, and quality of documentation in my spare time without it. Be assured that the design, concept, functional testing, and documentation proof-reading burned many human neuron hours.

I also built BgRaster to make it easy to have your LLM of choice help you with your complex configuations. Just point it as this documenation, describe what you're looking for, and have it generate the config file for you.

## Authors

<div style="display: flex; align-items: center; gap: 12px;">
	<img src="resources/gsp.svg" alt="Game Show Pro logo." style="height: 60px"/>
	<img src="https://www.barjonas.com/assets/img/barjonas.svg" alt="Barjonas LLC logo." style="height: 30px"/>
</div>

Hi, I'm Hamish Barjonas. I provide custom solutions for the broadcast producion, live entertainment, and sports industries. Yes, including game shows. See more details [here](https://www.barjonas.com). As a keen FOSS advocate, I try to keep as much non-customer-specific code open for the wider community as possible, under the Game Show Pro umbrella. If you're in a related industry, I'd love to colloborate! You can contact me [here](https://barjonas.com/contact).

## License

This project is licensed under the [MIT License](./LICENSE).
