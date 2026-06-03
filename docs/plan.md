# BgRaster development plan

BgRaster is a .net console app that is designed to run until its work complete then exit.

The headline function of the application is to assign custom backgrounds to every video output of the Windows machine it's running on. BgRaster renders one tightly-cropped PNG per physical output rather than a single desktop-span image. These PNGs are written into a BgRaster subdirectory under the OS temporary folder using filename-safe ISO 8601 UTC timestamps so every generated file is unique. After render, BgRaster assigns each PNG directly to its physical output using the COM `IDesktopWallpaper` interface. Older BgRaster-generated PNGs are then sent to the recycle bin; if any remain because Windows or another process still has them locked, they are left in place and retried on the next successful apply.

## Licensing
The appliction is published under the MIT license, (C) 2026 Hamish Barjonas

## Terminology

| Term | Definition |
|---|---|
| **output** | A single physical video output as enumerated by Windows (e.g. one monitor or projector connection). Corresponds to an `[[output]]` TOML object. |
| **slice** | A rectangular subdivision of an output used to render an independent visual region within one physical output. Corresponds to an `[[output.slice]]` TOML object. |
| **target** | The value used to match a config `[[output]]` object to a discovered physical output. Can be a zero-based OS index (integer) or a stable OS unique identifier string. |
| **viewport** | The coordinate space of the current rendering context. Within an output scope, viewport units are relative to that output's pixel dimensions. Within a slice scope, viewport units are relative to that slice's pixel dimensions. |
| **hardware profile** | A normalized, serializable snapshot of all connected outputs and their properties, built once per run from Windows APIs. Used for early-exit cache comparison. |
| **lastRun.toml** | Persisted file written after each successful non-no-assignment apply, containing the settings hash, executable version, serialized hardware profile, effective configuration model, and generated diagnostic comments for the run. Used to detect unchanged state and troubleshoot behavior without mutating the user config. |
| **lastRun.dry.toml** | The no-assignment equivalent of `lastRun.toml`. Written and read during no-assignment execution instead of `lastRun.toml`. |

## Image rendering
The desktop image will be composed and rendered using SkiaSharp for maximum performance and flexibility. It's desirable to use SVG semantics for building the image due to the universal familiarity, but that's not as important as performance.

## Parmameter handling
The primary configuration format is TOML. Commandline parameters are parsed by System.CommandLine and act as an override layer on top of TOML.

Effective value resolution order:

1. built-in defaults
1. TOML global array-based section
1. commandline overrides for CLI-enabled global properties
1. TOML per-output overrides
1. TOML per-slice overrides

Per-output and per-slice values intentionally override global values, including globally applied commandline values.

## Format choice
YAML was considered because it is comment-friendly, but TOML remains the chosen format for v1 because:

1. it maps cleanly to strongly typed .NET models
1. the section structure aligns with commandline prefixes
1. it is less ambiguous for scalar types and arrays

Generated status comments, effective-value comments, and run diagnostics are written to `lastRun.toml` each run. Existing user config TOML is treated as read-only input and is never modified automatically at runtime; the one exception is that when `--config` points to a missing file, BgRaster may create that new file only after a successful run by writing a seeded `config.toml` template.

### Config file location and schema linking
The application accepts an explicit config path via `--config path/to/file.toml`. If that path does not exist, the app runs from built-in defaults and CLI overrides; after a successful run it seeds the requested config path by writing a `config.toml` template with effective global defaults and detected output targets. If `--config` is omitted, it searches for `config.toml` in this order: next to the executable, `%ProgramData%\BgInfo`, `%LocalAppData%\BgInfo`, then `%AppData%\BgInfo`.

Every `config.toml` file should include a `$schema` comment at the top that points to the current published schema:

```toml
# $schema: https://raw.githubusercontent.com/gameshowpro/BgRaster/refs/heads/main/docs/schemas/bgraster-config.schema.json
```

If an existing `config.toml` contains a `$schema` comment that points somewhere else, the app may update only that `$schema` comment line to the published schema URL. If it's missing altogether, it may be added. No other config values, ordering, or formatting are modified.

### Naming convention
TOML section and key names map mechanically to long commandline options.

Mapping rule:

1. take section path and key path in kebab-case
1. join with hyphen
1. prefix with --

Examples:

1. [grid] size maps to --grid-size
1. [grid] odd-color maps to --grid-odd-color
1. [circle] stroke maps to --circle-stroke
1. [text] text maps to --text
1. [crosshair] length maps to --crosshair-length
1. [logo] width maps to --logo-width
1. [logo] opacity maps to --logo-opacity

Only properties designated as CLI-enabled are exposed as commandline options. Slice definitions remain TOML-only.

A key purpose of the CLI is to allow specific, reproducible test patterns to be expressed as a single publishable command line. Documentation can ship canonical example commands for common AV diagnostic scenarios, such as:

- alternating pixel pattern only (no grid, circle, crosshair, or text): `--background-alternating true --grid-size 0 --circle-size 0 --crosshair-length 0 --text-size 0`
- pure color flood with slice borders and crosshair: `--background-border true --crosshair-length 50vmin --circle-size 0 --text-size 0`
- full diagnostic overlay: alternating pixel background, 100px grid, crosshair, border, text, coordinates

CLI options must cover every global property that participates in these diagnostic patterns.

## Fields
Several fields are available for substitutions in string options:

1. MachineName
1. OutputWidth
1. OutputHeight
1. OutputIndex
1. OutputIndexPlusOne
1. OutputLetter
1. OutputName
1. SliceWidth (slice scope only)
1. SliceHeight (slice scope only)
1. SliceIndex (slice scope only)
1. SliceIndexPlusOne (slice scope only)
1. SliceLetter (slice scope only)

Fields are inserted using `${FieldName}` syntax.

## Units and color semantics
All distances can be expressed as px, vh, vw, vmin, vmax. If units are omitted, px is assumed.

Within an output scope, viewport units are relative to that output.
Within a slice scope, viewport units are relative to that slice.

Color inputs support CSS-compatible forms: #RRGGBB, #RRGGBBAA, rgb(), rgba(), hsl(), hsla().

## TOML schema

The schema is organized into global tables and per-output objects.

Scope and type rules:

1. Global table values that support cycling are arrays and map by rendered slice sequence with wraparound.
1. Outputs without explicit `[[output.slice]]` entries are treated as a single implicit full-output slice (`x=0`, `y=0`, `width=100vw`, `height=100vh`).
1. The same properties can be set as scalar overrides under each `[[output]]` object.
1. The same properties can be set as scalar overrides under each `[[output.slice]]` object.
1. Slice objects are TOML-only.
1. Output-level and slice-level scalar values override resolved global values.

### Canonical commented schema

The canonical source for TOML comments, defaults, allowed values, and field descriptions is the JSON Schema files:

1. `docs/schemas/bgraster-config.schema.json`
1. `docs/schemas/bgraster-lastrun.schema.json`

`plan.md` documents behavior and architecture. Schema-level field semantics must be maintained in the schema files, not duplicated in large commented TOML examples.

### Object and property catalog

The object/property catalog is fully defined by the JSON Schema files.

1. `docs/schemas/bgraster-config.schema.json` defines user configuration fields, defaults, enums, and descriptions.
1. `docs/schemas/bgraster-lastrun.schema.json` defines runtime-state fields (`meta`, `hardware_output`) plus effective-config tables.

### lastRun.toml schema

The `lastRun.toml` (and `lastRun.dry.toml`) files persist runtime state and diagnostics. They are organized into three sections to avoid collisions between hardware profile and config:

Both `lastRun.toml` and `lastRun.dry.toml` must include a `$schema` comment at the top that points to `bgraster-lastrun.schema.json` for the current build tag.

1. `[meta]` - persisted runtime metadata (version, hash, timestamp, cleanup results)
1. `[[hardware_output]]` - array of detected outputs (hardware profile)
1. `[text]`, `[background]`, `[grid]`, etc. - effective configuration model identical to config TOML structure

This design allows the effective config to be copied directly from `lastRun.toml` to a user config file without renaming keys or paths.

#### Metadata section: `[meta]`

```toml
[meta]
# Executable semantic version (SemVer2) from Nerdbank.GitVersioning.
version = "1.0.0+build.1"

# SHA-256 hash of the effective configuration model (post TOML+CLI merge).
# Used for early-exit detection and run-state tracking.
settingsHash = "a1b2c3d4..."

# ISO 8601 UTC timestamp of when this run occurred, for diagnostics.
timestamp = "2026-04-30T18:42:11.1234567Z"

# Map of hardware output IDs to successfully assigned PNG file paths.
assignedFiles = { }

# List of stale PNG files that could not be recycled (locked, permission denied).
unrecycledFiles = [ ]
```

#### Hardware profile section: `[[hardware_output]]`

Each discovered physical output is recorded as an entry in the `hardware_output` array. This information is read back on startup to detect hardware changes and determine early-exit eligibility.

```toml
[[hardware_output]]
# Stable PnP device instance path from DisplayConfigGetDeviceInfo.
id = "MONITOR\\SAM0C7F#..."

# Zero-based index assigned by sorting outputs by (desktopY, desktopX, id).
index = 0

# Virtual desktop position in pixels (top-left corner).
desktopX = 0
desktopY = 0

# True hardware pixel dimensions (not scaled by OS DPI factor).
widthPx = 1920
heightPx = 1080

# Logical DPI values from GetDpiForMonitor(MDT_EFFECTIVE_DPI).
dpiX = 96
dpiY = 96

# Rotation in degrees: 0, 90, 180, or 270.
rotation = 0

# Adapter device name (e.g. \\.\DISPLAY1); used for logging.
adapterName = "\\\\.\\DISPLAY1"

# Friendly monitor name; used in ${OutputName} substitution and diagnostics.
friendlyName = "SAMSUNG LS27A600"
```

#### Configuration sections (identical to config.toml)

Immediately following the hardware section, all configuration tables are serialized in their canonical form, mirroring the structure of the user config file. The config sections are identical in both the user config and `lastRun.toml`, enabling direct copy-paste transfer.

### JSON Schema files for IDE validation

BgRaster must deploy JSON Schema definitions so that VS Code and compatible editors can provide real-time validation and intellisense for both `config.toml` (user config) and `lastRun.toml` (runtime state).

**Schema file locations (deployed with executable):**

1. `schemas/bgraster-config.schema.json` - Schema for user `config.toml`
2. `schemas/bgraster-lastrun.schema.json` - Schema for `lastRun.toml` and `lastRun.dry.toml`

**Repository location (source):**

Both schema files are hand-crafted and maintained in the repository under `docs/schemas/`:

- `docs/schemas/bgraster-config.schema.json`
- `docs/schemas/bgraster-lastrun.schema.json`

**Deployment strategy:**

1. Source files are versioned in the repository under `docs/schemas/`.
2. During CI build/publish, copy both schema files to the output/publish directory (e.g. `publish/schemas/`).
3. Users download the schema files alongside or obtain them from a release distribution endpoint.
4. Documentation and quick-start guide reference the schema location so users can configure their editors.

**VS Code integration (via TOML language server extension configuration):**

BgRaster config files include a `$schema` comment at the top that points to the published schema URL. Users can also place schema associations in `.vscode/settings.json` for broader coverage:

```json
{
  "evenBetterToml.schema.associations": {
    "config.toml": "file://<path-to-schema>/bgraster-config.schema.json",
    "lastRun.toml": "file://<path-to-schema>/bgraster-lastrun.schema.json",
    "lastRun.dry.toml": "file://<path-to-schema>/bgraster-lastrun.schema.json"
  }
}
```

**Schema comment format in config files:**

Each generated or user-facing TOML file should include a `$schema` comment at the top pointing to the corresponding JSON Schema in GitHub:

```toml
# config.toml
# $schema: https://raw.githubusercontent.com/gameshowpro/BgRaster/refs/heads/main/docs/schemas/bgraster-config.schema.json

# lastRun.toml and lastRun.dry.toml
# $schema: https://raw.githubusercontent.com/gameshowpro/BgRaster/refs/heads/main/docs/schemas/bgraster-lastrun.schema.json
```

At runtime, BgRaster may normalize the `$schema` comment in `config.toml` to the published schema URL when it is present but mismatched. BgRaster must always write the published `$schema` comments when generating `lastRun.toml` and `lastRun.dry.toml`.

**Schema content structure:**

The JSON Schema files describe:
- All required and optional top-level tables (`[text]`, `[background]`, `[render]`, etc.)
- All valid properties within each table with type, default, minimum/maximum, and enum values for constrained fields
- Array-of-objects structure for `[[output]]` and `[[output.slice]]`
- For `lastRun.toml`: additional `[meta]` table and `[[hardware_output]]` array with all hardware fields
- Helpful descriptions and links to documentation for each property
- Examples of valid values

### Namespace collision avoidance

To ensure no collisions between hardware profile and config:

1. Hardware metadata is isolated in `[meta]` (user config has no `[meta]` table)
2. Hardware outputs are in `[[hardware_output]]` array (user config has no `hardware_output` key)
3. All config tables (`[text]`, `[background]`, `[grid]`, `[circle]`, `[crosshair]`, `[logo]`, `[render]`, `[[output]]`, `[[output.slice]]`) are identical in both files
4. No config section or property is named `meta`, `hardware_output`, `version`, `settingsHash`, `timestamp`, `assignedFiles`, or `unrecycledFiles`

This design allows the effective configuration to be safely copied from `lastRun.toml` to a new user config file with zero name/path adjustments.

Background rendering rules:

1. the solid color fill is always rendered first, covering the full viewport
1. if `background.image` resolves to a valid path to an existing, parseable image file, that image is rendered over the color layer using `background.fit` mode; non-uniform scaling is never allowed
1. if `background.image` is empty, no image layer is rendered
1. if `background.image` is non-empty but invalid, missing, or unparsable, the background image layer is skipped for that viewport and execution continues; a generated status comment should record the skip reason
1. fit mode semantics:
1. `BestFit` scales uniformly so the entire image is visible inside the viewport (letterboxing/pillarboxing permitted)
1. `CropToFill` scales uniformly so the viewport is fully covered, then crops overflow equally around center
1. `CropTL`, `CropTR`, `CropC`, `CropBL`, `CropBR` perform anchor-based cropping with no scaling; image pixels are sampled 1:1 and the viewport takes the anchored crop window
1. grid is rendered after background color and background image
1. if `background.alternating` is true, a pixel-alternating layer is rendered over grid: pixels where `(x + y) % 2 == 0` use the resolved `background.color`, and pixels where `(x + y) % 2 == 1` use pure black (`#000000`)
1. if `background.border` is true, a 1-pixel-wide border is drawn at the absolute outer edge of the viewport after color/image/grid/alternating layers, using `background.border-color`
1. the border occupies the outermost row, outermost column on each side; it does not inset by any amount
1. the border is drawn over the top of color/image/grid/alternating layers so it is always a solid, unmodified 1-pixel line

Text positioning rules:

1. `text.x` and `text.y` define the text anchor point in the current output or slice
1. `text` lines are centered on that anchor point as a text block

Logo rendering rules:

1. if `logo.source` is empty, logo rendering is skipped entirely for that viewport
1. if `logo.source` is non-empty but does not resolve to a valid path pointing to an existing, parseable file, the embedded fallback SVG resource (compiled into the application) is rendered in its place; a `# bg-raster: status=logo-fallback-used source="<original-value>"` comment is written to the TOML output object or slice
1. `logo.x`, `logo.y`, `logo.width`, and `logo.height` define a fit rectangle in the current output or slice
1. the logo is scaled to best fit within that rectangle while preserving aspect ratio
1. the fitted logo is centered within that rectangle
1. supported logo formats for user-supplied files are SVG and PNG
1. the source image alpha (PNG alpha channel or SVG transparency) is honored during rendering
1. `logo.opacity` applies an additional opacity multiplier after image decoding; `0` is fully transparent and `1` is fully opaque

Grid coordinate rendering rules (when `coordinates` is enabled):

1. coordinates are rendered only when `grid.coordinates` resolves to `true`
1. each square displays its top-left corner pixel position in the current output or slice
1. text consists of three lines: x-coordinate, a centered `x` character (divider), and y-coordinate
1. text size is automatically calculated to fit all three lines within the square height
1. text color is determined by luminance of the square's color: black for light squares, white for dark squares
1. x-coordinate is top-left aligned
1. `x` divider is centered
1. y-coordinate is bottom-right aligned
1. a small filled triangle is rendered in the top-left corner of each square to indicate that coordinates refer to that corner
1. coordinate text does not wrap; if it cannot fit, it is omitted

Crosshair rendering rules:

1. crosshair is centered in the current output or slice viewport
1. render only when both `length` and `stroke` resolve to non-zero values
1. if either `length` or `stroke` resolves to `0`, crosshair is not rendered
1. crosshair consists of one horizontal and one vertical line intersecting at the exact center

## Render order
1. Background color layer
1. Background image layer (`background.image`, if present and valid)
1. Grid
1. Alternating pixel layer (`background.alternating`, if enabled)
1. Background border (1-pixel outer edge, if enabled)
1. Circle
1. Crosshair
1. Labeled edges
1. Logo
1. Text

## Matching and discovery
At runtime, the app enumerates outputs from the OS and associates each discovered output with at most one output object.

Matching logic:

1. try exact match by string target against OS unique identifier
1. for numeric targets, match by zero-based OS index
1. if multiple config objects match the same discovered output, the first valid match wins and others are marked duplicate ignored
1. if a config output target does not match a discovered output, mark output not found; this is tolerated — execution continues with the remaining matched outputs, and the not-found status is recorded as a generated comment in `lastRun.toml`

## Runtime diagnostics state
The config file is read-only at runtime. BgRaster writes all generated diagnostics, happy-path comments, effective values, and persisted run-state data to `lastRun.toml` or `lastRun.dry.toml`.

Required `lastRun` write behavior:

1. `lastRun.toml` must contain the effective configuration model exactly as used to compute the settings hash, after TOML load and CLI overlay are both applied
1. `lastRun.toml` must contain generated comments for success paths as well as warnings and failures so a healthy run is still diagnosable after the fact
1. `lastRun.toml` must contain per-output and per-slice validation, matching, render, assignment, and cleanup comments for every object BgRaster evaluates
1. `lastRun.toml` must contain generated file-lifecycle comments, including current output file names and any stale files that could not yet be recycled
1. output and slice entries in `lastRun.toml` must be ordered canonically for stable diffs and deterministic hashing
1. the user config file must never be rewritten, reformatted, or augmented at runtime

`lastRun` serialization requirements:

1. `lastRun.toml` may be re-serialized from the canonical in-memory model on every run; preserving prior whitespace or comment placement is not required
1. generated comments in `lastRun.toml` are owned entirely by BgRaster and are regenerated on each run
1. if writing `lastRun.toml` produces a document that does not parse back to the same logical value tree, abort the write and log an error rather than silently corrupting the file
1. the implementation must have unit tests that serialize and deserialize `lastRun.toml` and assert stable ordering, stable comments for identical runs, and lossless round-tripping of the effective configuration model and diagnostics data

## Generated status comments
Each output record in `lastRun.toml` begins with a generated status comment from the most recent run.

Examples:

1. # bg-raster: status=output-not-found
1. # bg-raster: status=output-discovered id="SAMSUNG 1234D" index=0 position=0,0 resolution=1920x1080
1. # bg-raster: status=duplicate-output-ignored

Each slice record in `lastRun.toml` may also include a generated status comment.

Examples:

1. # bg-raster: status=slice-out-of-bounds reason="slice rect (x=0,y=900,w=1920,h=200) exceeds output bounds (1920x1080)"
1. # bg-raster: status=slice-rendered

Generated status comment policy:

1. comments are regenerated each run
1. comments summarize validation and matching status
1. comments do not alter effective values
1. comments are written only to `lastRun.toml` or `lastRun.dry.toml`, never to the user config file
1. happy-path comments are allowed and encouraged when they help explain what BgRaster rendered, matched, assigned, or cleaned up

## CLI availability
All global properties may be defined in TOML.

A subset is available on commandline. For now:

1. global text, background, grid, circle, crosshair, logo, and render properties are CLI-eligible
1. per-output objects are TOML-only
1. slices are TOML-only

## Implementation model
Use one hierarchical options model mirroring the TOML schema:

1. GlobalOptions
1. OutputOptions[]
1. SliceOptions[] under each OutputOptions
1. HardwareProfile
1. LastRunState

Processing pipeline:

1. load defaults
1. load TOML
1. apply CLI overlay to CLI-enabled global properties
1. compute SHA-256 settings hash from the serialized in-memory options model (captures the merged result of TOML and CLI; this is the hash stored in the run cache)
1. query Windows display APIs once and normalize the results into a canonical `HardwareProfile` object model containing only the fields required by later stages
1. load `lastRun.dry.toml` (if `--no-assignment`) or `lastRun.toml` (otherwise) if present and compare stored executable version, stored settings hash, and stored `HardwareProfile` against current values
1. if not `--no-assignment` and executable version, settings hash, and `HardwareProfile` are all unchanged, exit early without rendering or wallpaper apply
1. build output mapping via target using only the canonical `HardwareProfile`
1. resolve effective options per output
1. resolve effective options per slice
1. render one PNG per matched physical output into the output directory using a filename-safe ISO 8601 UTC timestamp in each file name
1. unless `--no-assignment`, assign each PNG to its target monitor ID using `IDesktopWallpaper.SetWallpaper`; if `--no-assignment`, skip the platform call entirely
1. after a successful non-no-assignment assignment, enumerate older BgRaster-generated files in the output directory and send all files not produced by the current run to the recycle bin; if a file cannot be recycled because it is locked or access is denied, leave it in place and record the failure in `lastRun.toml`
1. write `lastRun.toml` (non-no-assignment) or `lastRun.dry.toml` (no-assignment) containing executable version, settings hash, serialized `HardwareProfile`, effective configuration model, generated comments, current output file list, and stale-file cleanup results

This keeps the user config stable while still persisting enough resolved runtime information to debug behavior and support deterministic early-exit decisions.

## Technical implementation requirements

Build and deployment goals:

1. target .NET 10
1. publish as framework-independent (self-contained) executable
1. publish as single-file executable
1. enable trimming at the highest safe level
1. enable Native AOT
1. ship Windows x64 first (win-x64), with win-arm64 as optional second target

Required project publish settings:

1. `SelfContained=true`
1. `PublishSingleFile=true`
1. `PublishTrimmed=true`
1. `TrimMode=full`
1. `PublishAot=true`
1. `InvariantGlobalization=true`
1. `DebuggerSupport=false`
1. `MetadataUpdaterSupport=false`
1. `StackTraceSupport=true` for release diagnostics
1. `DisableRuntimeMarshalling=true` unless a specific dependency requires runtime marshalling

Package requirements:

1. `SkiaSharp` for rendering primitives, text, and image encoding
1. `SkiaSharp.NativeAssets.Win32` for native Skia binaries on Windows
1. `System.CommandLine` for CLI parsing and option validation
1. `Tomlyn` for TOML parsing and `lastRun` read/write

Package usage constraints for trim and AOT safety:

1. do not use reflection-based object mappers for TOML; parse into TOML model and map manually
1. avoid runtime type discovery, dynamic loading, and expression compilation
1. avoid dependency injection containers in v1; use explicit constructors and composition root in Program
1. keep all option models as plain records/classes with explicit defaults
1. keep all serialization and parsing code deterministic and culture-invariant

Embedded resource requirements:

1. compile `resources\gidole\Gidolinya-Regular.otf` into the assembly as an embedded resource by adding `<EmbeddedResource Include="resources\gidole\Gidolinya-Regular.otf" />` to `BgRaster.csproj`
1. at startup, load the font typeface via `Assembly.GetExecutingAssembly().GetManifestResourceStream(...)` (AOT-safe manifest stream access) and register it with SkiaSharp's custom font manager; it is the only font used for all text rendering and there is no system font fallback in v1
1. compile a fallback SVG logo as an embedded resource; it must be a simple but recognizable placeholder design (e.g. diagonal cross inside a border rectangle) so that users notice it is not their real logo
1. both embedded resources must be accessed exclusively via `Assembly.GetManifestResourceStream` at runtime; no file-system path is constructed for them

Runtime architecture requirements:

1. `Program` contains only startup orchestration and exit-code handling
1. configuration layer handles defaults and TOML load plus CLI overlay; it does not mutate the user config file
1. discovery layer handles one-time Windows API enumeration and normalization into `HardwareProfile`
1. state cache layer loads and writes `lastRun.toml` (or `lastRun.dry.toml` for no-assignment)
1. resolution layer computes effective options for each output and slice
1. rendering layer draws one tightly-cropped bitmap per matched output in strict render order
1. wallpaper layer assigns generated PNGs to monitors via COM `IDesktopWallpaper`
1. file lifecycle layer generates unique timestamped file names and recycles stale prior-run files after successful assignment
1. target matching uses only `HardwareProfile` and must not query Windows APIs again after normalization
1. diagnostics layer emits concise logs and generated status comments into `lastRun.toml`

Hardware profile and early-exit requirements:

1. the first hardware-dependent step in every run must be a single Windows API query pass that is normalized into a canonical `HardwareProfile` object model
1. `HardwareProfile` must contain only fields that matter to later behavior: stable output identifier, output name, width, height, x, y, DPI, rotation, and any other fields required for rendering, targeting, or status reporting
1. once `HardwareProfile` is created, no later pipeline step may query Windows display APIs again; all later logic must read from the normalized object model
1. `HardwareProfile` serialization must be deterministic and order-independent by sorting outputs by stable identifier before serialization
1. BgRaster must persist a `lastRun.toml` file after each successful non-no-assignment apply; a no-assignment writes `lastRun.dry.toml` instead
1. both `lastRun.toml` and `lastRun.dry.toml` must contain at minimum: executable semantic version, settings hash, serialized `HardwareProfile`, effective configuration model, generated comments, and generated output file metadata
1. the settings hash is a SHA-256 of the serialized in-memory options model computed after TOML load and CLI overlay are both applied; it is not a hash of raw TOML file bytes; CLI-only overrides that change the in-memory model change the hash and prevent an early-exit skip
1. early-exit comparison is eligible only for non-no-assignment execution
1. early exit occurs only when executable version, settings hash, and serialized `HardwareProfile` all match the values in `lastRun.toml`
1. when the cached state matches current state, exit with success and log `status=run-skipped-unchanged`
1. if `lastRun.toml` (or `lastRun.dry.toml` for no-assignment) is missing, unreadable, or schema-incompatible, continue normal execution and regenerate it after a successful apply
1. generated output file names must use a filename-safe ISO 8601 UTC timestamp component, for example `2026-04-30T18-42-11.1234567Z`, plus a stable sanitized output identifier suffix so each file name is unique and traceable to its target output
1. `lastRun.toml` must record the exact file path assigned to each output so stale-file cleanup can distinguish current-run files from recyclable leftovers

Windows integration requirements:

1. embed an application manifest declaring `PerMonitorV2` DPI awareness so Windows reports true hardware pixel dimensions for every output, regardless of the UI scaling factor configured by the OS or user
1. the manifest must set `<dpiAware>true/pm</dpiAware>` and `<dpiAwareness>PerMonitorV2</dpiAwareness>` in the `windowsSettings` element
1. if the process DPI awareness cannot be confirmed at startup, log a warning and abort rather than silently producing an incorrectly-sized image
1. use source-generated interop via `LibraryImport` (not `DllImport`) for Windows API entry points
1. define interop methods as `static partial` with explicit `StringMarshalling = Utf16` where applicable
1. set `SetLastError = true` on interop signatures that report Win32 errors
1. do not use `SystemParametersInfo` for wallpaper assignment in v1; use COM `IDesktopWallpaper` exclusively so BgRaster can assign one generated PNG directly to each physical output by monitor ID
1. use `IDesktopWallpaper.GetMonitorDevicePathCount`, `GetMonitorDevicePathAt`, and `SetWallpaper` to map each rendered file to its physical output ID
1. if a wallpaper assignment call fails for any output, BgRaster must attempt to clear wallpaper assignment for the affected outputs using `IDesktopWallpaper.SetWallpaper(monitorId, null)` or equivalent empty-path semantics, then exit non-zero and log the failure in `lastRun.toml`
1. stale generated files must be sent to the recycle bin using Windows shell file-operation APIs rather than permanently deleted; recycle failures caused by file locks are non-fatal and must be recorded for retry on a future run
1. return non-zero exit code on API failure and log last Win32 error
1. keep all Windows-only code in a dedicated interop module
1. avoid custom marshallers in v1 unless a required API cannot be expressed with built-in marshalling

AOT and trim validation requirements:

1. CI must run `dotnet publish` with Native AOT and full trimming enabled
1. produced executable must run on a clean Windows machine without installed .NET runtime
1. smoke test must cover: config load, output discovery, per-output PNG render, timestamped file naming, no-assignment path, and stale-file cleanup planning
1. release build must fail CI if trimming or AOT analysis introduces warnings in BgRaster code

Performance and quality requirements:

1. do not allocate a single giant desktop-span bitmap in v1; render one output bitmap at a time and dispose it promptly after encode/write
1. avoid per-cell heap allocations in grid rendering hot paths
1. decode logo assets once per render and reuse decoded bitmap/picture
1. keep render output deterministic for identical inputs
1. include golden-image regression tests for at least one multi-display fixture

## Version numbering

Versioning is managed by [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) (NBGV).

Version number requirements:

1. add `Nerdbank.GitVersioning` as a build-time package reference in `BgRaster.csproj` with `PrivateAssets=all`
1. place a `version.json` file at the repository root defining the base `version` and `semVer1NumericIdentifierPadding`
1. NBGV derives the full SemVer2 version automatically from the git commit height on the active branch
1. the `AssemblyInformationalVersion` attribute baked into the executable must contain the full SemVer2 string including any pre-release label and git commit hash
1. the `AssemblyVersion` and `FileVersion` contain `Major.Minor.Patch.Height` with no pre-release label

Required `version.json` at repository root:

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/main/src/NerdBank.GitVersioning/version.schema.json",
  "version": "1.0",
  "semVer1NumericIdentifierPadding": 4
}
```

CI requirements for NBGV:

1. install the `nbgv` global tool in the CI workflow before build: `dotnet tool install -g nbgv`
1. run `nbgv cloud` early in CI to expose version environment variables to subsequent steps
1. create a git tag for the exact version being built (for example `v1.2.3`) and attach it to the current commit before publish steps that stamp schema URLs
1. push the created version tag to the repository when running on push events for the default branch; skip tag creation on pull_request builds
1. use the computed version to tag build artifacts (the published executable directory name or archive name should include the version string)

## Continuous integration requirements

The repository must include a GitHub Actions workflow at `.github/workflows/ci.yml`.

Workflow requirements:

1. trigger on push and pull_request to main development branches
1. run on `windows-latest`
1. install .NET 10 SDK
1. install and run `nbgv cloud` before any build steps to set version environment variables
1. compute the release tag name from the build version and create/attach that tag to `GITHUB_SHA`
1. push the release tag when the workflow runs on a push to the main branch
1. run restore, build, and test for the solution
1. run publish for `win-x64` with self-contained single-file full-trim Native AOT settings
1. run a second publish for `win-arm64` when enabled
1. fail the build on warnings from BgRaster code (`TreatWarningsAsErrors=true` for CI)
1. upload publish artifacts for each RID

Minimum CI command set:

1. `dotnet restore`
1. `dotnet build -c Release`
1. `dotnet test -c Release --no-build`
1. `dotnet publish src/BgRaster.csproj -c Release -r win-x64 /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TrimMode=full /p:PublishAot=true`

## Test project requirements

The repository must include a dedicated test project (for example `tests/BgRaster.Tests`) and add it to the solution.

Required test stack:

1. `xunit` as the unit test framework
1. `xunit.runner.visualstudio` for test discovery in CI and IDE
1. `FluentAssertions` for readable assertions

Required test coverage areas:

1. option resolution precedence (defaults -> global -> CLI -> output -> slice)
1. unit parsing (`px`, `vh`, `vw`, `vmin`, `vmax`) and scope-relative calculations
1. color parsing for all documented CSS-compatible formats
1. target matching behavior for string IDs and numeric indexes
1. grid coordinate rendering rules, including marker triangle and contrast color selection
1. background image fit behavior (`CropTL`, `CropTR`, `CropC`, `CropBL`, `CropBR`, `BestFit`, `CropToFill`) with explicit assertions that non-uniform scaling is never used
1. render order correctness (background color -> background image -> grid -> alternating -> background border -> circle -> crosshair -> logo -> text)
1. `lastRun.toml` generation behavior for effective configuration capture, generated comments, output file metadata, and stale-file cleanup results
1. no-assignment behavior and output-path behavior
1. `HardwareProfile` normalization and deterministic serialization
1. `lastRun.toml` based early-exit behavior, including executable version, settings hash, hardware profile comparison, and effective-configuration capture

Golden image verification requirements:

1. include at least one deterministic fixture with known config and expected PNG output
1. compare output image bytes or per-pixel data with an explicit tolerance policy
1. run golden tests only in environments that have required native dependencies available

## Documentation requirements

Project documentation must be written to the standard expected of a widely-used public open-source project.

Documentation deliverables:

1. repository root `README.md` with a clear project overview, feature summary, and support matrix (Windows versions, output architectures)
1. quick-start guide in `README.md` covering install/build, first run, no-assignment verification, and wallpaper apply flow
1. sample command lines in `README.md` for common AV diagnostics (pure color, alternating pixel pattern, border validation, coordinate grid, logo placement)
1. screenshots in `README.md` showing at minimum: pure color output, alternating pixel output, grid with coordinates, and bordered slices
1. link from `README.md` to detailed documentation pages for full TOML schema and full CLI schema
1. detailed TOML schema reference page (for example `docs/toml-schema.md`) that documents every table, key, type, default, units, and examples
1. detailed CLI schema reference page (for example `docs/cli-schema.md`) that documents every command, option, expected type, defaults, and examples
1. troubleshooting page (for example `docs/troubleshooting.md`) covering DPI awareness issues, output matching failures, `lastRun.toml` diagnostics, stale-file recycle behavior, and early-exit fingerprint behavior
1. architecture page (for example `docs/architecture.md`) describing runtime pipeline, layering, and data flow between TOML parsing, output discovery, resolution, per-output render, wallpaper assignment, `lastRun` persistence, and stale-file cleanup

Documentation quality requirements:

1. every CLI option documented in the CLI schema must have a corresponding TOML key mapping where applicable
1. every key in the TOML schema must be represented in the canonical example config and in the reference page
1. command examples must be copy-paste runnable and tested in CI or documented as manually verified
1. screenshots must match the documented defaults and include a caption describing what behavior they validate
1. all docs must avoid ambiguous wording like "may" where behavior is deterministic in code

## Resolved design decisions

All behavioral edge cases are locked as follows. Implementing code must follow these exactly.

1. **output identity source**: use `monitorDevicePath` from `DisplayConfigGetDeviceInfo(DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME)` as the stable unique output ID for `target` string matching. This is the PnP device instance path (e.g. `\\?\DISPLAY#SAM0C7F#5&1a2b3c4d&0&UID256#{e6f07b5f-...}`) that survives adapter renumbering and driver reinstalls. As supplementary fields, also capture `monitorFriendlyDeviceName` and `DISPLAY_DEVICE.DeviceName` (adapter path, e.g. `\\.\DISPLAY1`), but use only `monitorDevicePath` as the primary ID.

1. **output ordering**: assign `index` values by sorting outputs by `(desktopY ascending, desktopX ascending, monitorDevicePath ascending as tiebreaker)`. This matches the left-to-right, top-to-bottom reading order Windows uses in its display settings UI and is stable for identical hardware layouts. Use this order everywhere: rendering, hashing, status comments.

1. **hardware profile fields**: the canonical `OutputRecord` in `HardwareProfile` contains exactly these fields:

   | Field | Type | Windows API source |
   |---|---|---|
   | `id` | string | `DisplayConfigGetDeviceInfo` → `DISPLAYCONFIG_TARGET_DEVICE_NAME.monitorDevicePath` |
   | `index` | int | computed: position in sort order (desktopY asc, desktopX asc, id asc) |
   | `desktopX` | int | `EnumDisplaySettingsEx` → `DEVMODE.dmPosition.x` |
   | `desktopY` | int | `EnumDisplaySettingsEx` → `DEVMODE.dmPosition.y` |
   | `widthPx` | int | `EnumDisplaySettingsEx` → `DEVMODE.dmPelsWidth` |
   | `heightPx` | int | `EnumDisplaySettingsEx` → `DEVMODE.dmPelsHeight` |
   | `dpiX` | int | `GetDpiForMonitor(MDT_EFFECTIVE_DPI)` X component |
   | `dpiY` | int | `GetDpiForMonitor(MDT_EFFECTIVE_DPI)` Y component |
   | `rotation` | int | `EnumDisplaySettingsEx` → `DEVMODE.dmDisplayOrientation` (degrees: 0/90/180/270) |
   | `adapterName` | string | `EnumDisplayDevices` → `DISPLAY_DEVICE.DeviceName` (e.g. `\\.\DISPLAY1`); used in logging only |
   | `friendlyName` | string | `DisplayConfigGetDeviceInfo` → `monitorFriendlyDeviceName`; used in status comments and `${OutputName}` substitution |

   All string fields are serialized with invariant culture. When writing `HardwareProfile` to `lastRun.toml`, outputs are sorted by `id` (not by `index`) to guarantee a deterministic serialized form regardless of enumeration order.

1. **settings hash**: a SHA-256 hash of the serialized in-memory options model computed after TOML load and CLI overlay are both applied. It is not a hash of the raw TOML file bytes. CLI-only overrides that change the in-memory model change the hash and prevent an early-exit skip.

1. **CLI override interaction with run cache**: because the hash is computed from the post-merge in-memory model, a CLI-only override (without any TOML file change) changes the hash and prevents a run-skipped-unchanged skip. This is the intended behavior.

1. **no-assignment semantics**: no-assignment skips the platform call to set the desktop wallpaper entirely. It reads and writes `lastRun.dry.toml` instead of `lastRun.toml`. no-assignment is never eligible for early-exit skip.

1. **output-not-found policy**: unmatched configured outputs are tolerated. Execution continues with the remaining matched outputs. The outcome is recorded in a generated `lastRun.toml` comment. No warning or error is emitted unless verbosity is verbose.

1. **overlapping slices policy**: overlapping slices are allowed. They are rendered in the order they appear in the TOML file. No warning is emitted.

1. **out-of-bounds slice policy**: a slice whose bounds extend outside its parent output bounds is dropped from rendering. Execution continues with the remaining valid slices. The dropped slice receives a `# bg-raster: status=slice-out-of-bounds reason="<description of out-of-range bounds>"` comment.

1. **text shaping and font**: `Gidolinya-Regular.otf` embedded from `resources\gidole\Gidolinya-Regular.otf` is the only font used for all text rendering. There is no system font fallback in v1. Glyphs absent from Gidolinya are rendered as empty space without error.

1. **logo decode failure policy**: if `logo.source` is non-empty but does not point to a valid, existing, parseable file, the built-in fallback SVG resource is rendered in its place. A generated `lastRun.toml` comment records `status=logo-fallback-used source="<original-value>"`. Execution continues.

1. **wallpaper apply failure**: if `IDesktopWallpaper` fails to assign a rendered file to any output, BgRaster immediately attempts to clear wallpaper assignment for the affected outputs using `IDesktopWallpaper` null/empty-path semantics. This ensures no stale wallpaper is left that could be mistaken for a successfully applied new image. BgRaster then exits with a non-zero exit code, logs the failure, and records the cleanup outcome in `lastRun.toml`.

1. **locked wallpaper files**: generated PNG file names must include a filename-safe ISO 8601 UTC timestamp so every run produces unique files. After a successful assignment, BgRaster sends all older BgRaster-generated files not referenced by the current run to the recycle bin. If any file remains because it is locked, that condition is non-fatal; the file stays in place, is recorded in `lastRun.toml`, and is retried on a later run.

## Possible future development
1. **Config import command** — a CLI sub-command (`import`) that reads a config file from another system and converts it to a BgRaster TOML file. Possible targets: Ventuz render setup files (.vren), Novastar (VideoWall/NovaLCT JSON/XML project files) and Resolume Avenue/Arena composition files. The import maps each output or layer region to an `[[output]]` or `[[output.slice]]` object with inferred `target`, geometry, and color values.

1. **Windows service mode** — optional installation as a Windows service that listens for OS display change events (WM_DISPLAYCHANGE) and system startup events and reruns the core pipeline automatically when outputs are connected, disconnected, or reconfigured.

1. **Preview window** — a `--preview` flag that opens a scaled-down Win32 window showing the generated image before applying it, for interactive configuration without a full wallpaper commit.

1. **HTTP trigger** — a lightweight embedded HTTP endpoint that accepts a POST request to trigger a re-render, enabling integration with show-control systems such as QLab, Bitfocus Companion, or custom automation scripts. Command line argument could be provided in the post data.
