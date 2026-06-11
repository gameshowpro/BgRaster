# Future plans

This page records deveopment work that is planned or under consideration.

## Recycle Bin support for stale outputs

Done:
- Stale `.png` files are detected with `StaleFileCleaner.FindStaleFiles`.
- Stale file intent is logged so the retry path remains visible.

Not done:
- The Windows shell recycle-bin integration is still stubbed.
- `IFileOperation` / COM-backed recycle behavior is not implemented yet.
- Stale files are returned to the caller for retry on the next run.

## Golden image regression coverage

Done:
- The renderer is stable enough to support pixel comparison tests.
- The codebase already has focused unit tests around parsing, resolution, and hashing.

Not done:
- Reference image fixtures are not yet established.
- There is no golden comparison harness for rendered outputs.
- There is no tolerance or diff-report workflow for visual regressions.

## Native AOT validation for rendering dependencies

Done:
- The codebase has already been pushed toward AOT-friendly patterns.
- SkiaSharp usage is isolated behind rendering layers and helper types.

Not done:
- `DisableRuntimeMarshalling` has not been enabled globally.
- The publish/test matrix against the pinned SkiaSharp version still needs explicit validation.
- The unsafe pixel-access path in `AlternatingLayer` remains the most likely breakage point.

## Config import command

A CLI sub-command (`import`) that reads a config file from another system and converts it to a BgRaster TOML file. Possible targets: Ventuz render setup files (.vren), Novastar (VideoWall/NovaLCT JSON/XML project files) and Resolume Avenue/Arena composition files. The import maps each output or layer region to an `[[output]]` or `[[output.slice]]` object with inferred `target`, geometry, and color values.

## Windows service mode

Optional installation as a Windows service that listens for OS display change events (WM_DISPLAYCHANGE) and system startup events and reruns the core pipeline automatically when outputs are connected, disconnected, or reconfigured.

## HTTP trigger
A lightweight embedded HTTP endpoint that accepts a POST request to trigger a re-render, enabling integration with show-control systems such as QLab, Bitfocus Companion, or custom automation scripts. Command line argument could be provided in the post data.

## Chocolatey package
Build should push BgRaster package to Chocolatey community repository for easy installation/upgrade from commandline or GUI tools. Binaries would be embedded in the package. This would automatically make BgRaster available on the path.

## More BgInfo features
Although not the focus of this application, there are some features from BgInfo that could still be useful. In particular, new substitution variables to allow text to include:
* Enumeration of graphics hardware
* Processor name and core details
* Network adapters and IP addresses

## Named template "super switches"
Develop a list set of rich templates targetted at different use-cases. The local config can be seeded by including running once with the switch, like `--template-led` or `--template-projector`.