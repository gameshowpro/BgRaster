# TOML Schema Reference

BgRaster reads its primary configuration from a TOML file (default: `config.toml` next to the executable; override with `--config`). Every key is optional — missing keys fall back to the documented defaults. CLI flags override the corresponding TOML values; per-output overrides take precedence over globals; per-slice overrides take precedence over per-output.

## Conventions

- **All multi-value globals are arrays.** `BgRaster` cycles through array elements per output index using `array[index % array.Length]`. Specifying a single-element array applies the value to every output.
- **Dimension strings** accept the units listed under [Units](#units).
- **color strings** accept the formats listed under [colors](#colors).
- **Field substitution** is applied to text and image-source values — see [Substitution tokens](#substitution-tokens).
- **TOML keys use kebab-case** (e.g. `border-color`, `grid-coordinates`); the C# model uses PascalCase (`BorderColor`, `GridCoordinates`).

---

## `[text]`

Diagnostic text lines rendered on top of every output.

| Key | Type | Default | Description |
|---|---|---|---|
| `text` | `string[]` | `["${MachineName} ${Index}", "${OutputName}", "${Width}x${Height}"]` | Text lines. Substitution applied. |
| `size` | `string[]` | `["3vh", "2vh", "4vh"]` | Per-line font heights. Dimension. |
| `color` | `string[]` | `["#fff"]` | Per-line text colors. |
| `x` | `string[]` | `["75vw"]` | Anchor X (left edge of text block). Dimension. |
| `y` | `string[]` | `["75vh"]` | Anchor Y (baseline of first line). Dimension. |

---

## `[background]`

Solid color and optional bitmap image fill, plus alternating-pixel and border modes.

| Key | Type | Default | Description |
|---|---|---|---|
| `color` | `string[]` | `["#FF0000", "#00FF00", "#0000FF"]` | Fill color. Cycled per output. |
| `image` | `string[]` | `[""]` | Path to PNG/JPG. Empty disables. |
| `fit` | `string[]` | `["CropToFill"]` | One of: `CropTL`, `CropTR`, `CropC`, `CropBL`, `CropBR`, `BestFit`, `CropToFill`. |
| `alternating` | `bool[]` | `[false]` | When `true`, replaces background with checkerboard at pixel granularity (signal integrity test). |
| `border` | `bool[]` | `[false]` | When `true`, draws a 1-px outline at viewport edges. |
| `border-color` | `string[]` | `["#FFFFFF"]` | Border color. |

### `fit` modes
- `CropTL` / `CropTR` / `CropC` / `CropBL` / `CropBR` — image rendered at its native size, anchored to that corner; excess is cropped, gaps remain transparent.
- `BestFit` — uniform scale to fit entirely inside viewport (letterbox / pillarbox).
- `CropToFill` — uniform scale to cover viewport completely; excess is cropped from centre.

---

## `[grid]`

Regular grid overlay with optional per-cell coordinate labels.

| Key | Type | Default | Description |
|---|---|---|---|
| `size` | `string[]` | `["100px"]` | Cell side length. Dimension. `0` disables grid. |
| `odd-color` | `string[]` | `["#00000080"]` | color for cells where `(col+row) % 2 == 1`. |
| `even-color` | `string[]` | `["transparent"]` | color for cells where `(col+row) % 2 == 0`. |
| `stroke` | `string[]` | `["0"]` | Outline width. `0` = filled cells; `>0` = stroked outlines only. |
| `offset-x` | `string[]` | `["0"]` | X shift of grid origin. Dimension. |
| `offset-y` | `string[]` | `["0"]` | Y shift of grid origin. Dimension. |
| `coordinates` | `bool[]` | `[false]` | When `true`, each cell renders its `(col, row)` index plus a parity triangle in luminance-aware contrasting color. Cells smaller than 12 px skip the overlay. |

---

## `[circle]`

Centred circle (useful for verifying aspect ratio and projector keystone).

| Key | Type | Default | Description |
|---|---|---|---|
| `size` | `string[]` | `["100vmin"]` | Diameter. `0` disables. |
| `color` | `string[]` | `["#ffffff40"]` | Fill or stroke color. |
| `stroke` | `string[]` | `["0"]` | `0` = filled circle; `>0` = stroked outline. |

---

## `[crosshair]`

Centred plus-sign for alignment.

| Key | Type | Default | Description |
|---|---|---|---|
| `length` | `string[]` | `["5vmin"]` | Half-length of each arm. `0` disables. |
| `color` | `string[]` | `["#ffffff80"]` | Stroke color. |
| `stroke` | `string[]` | `["1px"]` | Arm thickness. `0` disables. |

---

## `[logo]`

Optional logo (PNG, JPG, or minimal SVG subset).

| Key | Type | Default | Description |
|---|---|---|---|
| `source` | `string[]` | `[""]` | Path to image file. Empty uses the embedded fallback (orange diagonal cross). Substitution applied. |
| `x` | `string[]` | `["75vw"]` | Logo center X. Dimension. |
| `y` | `string[]` | `["25vh"]` | Logo center Y. Dimension. |
| `width` | `string[]` | `["15vw"]` | Logo rect width. Dimension. |
| `height` | `string[]` | `["15vh"]` | Logo rect height. Dimension. |
| `opacity` | `float[]` | `[1.0]` | Alpha multiplier `0..1`. |

The logo is rendered into the rect using `BestFit` (uniform scale, preserves aspect ratio).

**SVG support** is intentionally compact and AOT-safe: `<rect>`, `<line>`, and `<path>` with commands `M/m`, `L/l`, `H/h`, `V/v`, `Z/z`, `C/c`, `S/s`, `Q/q`, and `T/t`, plus `fill`, `stroke`, `stroke-width`, `opacity`, `style`, and inherited group paint attributes. For light/dark theming, `fill` and `stroke` support the standard CSS color function `light-dark(lightColor, darkColor)`, and BgRaster selects the branch using background luminance. SVGs that fail to parse fall back to the embedded default logo.

---

## `[render]`

Run-mode scalars.

| Key | Type | Default | Description |
|---|---|---|---|
| `no-assignment` | `bool` | `false` | When `true`, generate PNGs but do not assign wallpaper or recycle stale files. State persists to `lastRun.dry.toml`. |
| `outputs-skip-unspecified` | `bool` | `false` | When `true`, only explicitly targeted `[[output]]` entries are rendered; discovered outputs without a matching entry are ignored. |
| `output` | `string` | `""` (→ `%TEMP%\BgRaster`) | Output directory for generated PNGs and `lastRun*.toml`. |
| `verbosity` | `string` | `"normal"` | One of `quiet`, `normal`, `verbose`. |
| `force` | `bool` | `false` | When `true`, continue rendering even after emitting `run-skipped-unchanged`. |

---

## `[[output]]` (array of tables)

Per-output configuration. Optional; outputs without an entry use the global cycled values.

| Key | Type | Description |
|---|---|---|
| `target` | `int` *or* `string` | Required. Integer = output index in desktop order; string = exact `OutputRecord.Id` (the `\\?\DISPLAY#...` device path). |
| `text` | inline table | Optional override; any of `title`, `subtitle`, `size`, `x`, `y`. |
| `background` | inline table | Optional override; any of `color`, `image`, `fit`, `alternating`, `border`, `border-color`. |
| `grid` | inline table | Optional override; any of `size`, `odd-color`, `even-color`, `stroke`, `offset-x`, `offset-y`, `coordinates`. |
| `circle` | inline table | Optional override; any of `size`, `color`, `stroke`. |
| `crosshair` | inline table | Optional override; any of `length`, `color`, `stroke`. |
| `logo` | inline table | Optional override; any of `source`, `x`, `y`, `width`, `height`, `opacity`. |
| `slice` | array of tables | Optional list of sub-rectangles (see below). When present, the output is rendered as N independent slices instead of a single full-output canvas. |

The first matching `[[output]]` wins; subsequent entries with the same target are reported as `duplicate-output-ignored`.

---

## `[[output.slice]]` (nested array of tables)

A rectangular sub-region of an output. Each slice gets its own substitution context (its width/height become `${Width}` / `${Height}`).

| Key | Type | Default | Description |
|---|---|---|---|
| `x` | `string` | `"0"` | Slice left edge within output. Dimension. |
| `y` | `string` | `"0"` | Slice top edge within output. Dimension. |
| `width` | `string` | `"100vw"` | Slice width. Dimension. |
| `height` | `string` | `"100vh"` | Slice height. Dimension. |
| `text` / `background` / `grid` / `circle` / `crosshair` / `logo` | inline table | — | Optional overrides, same shape as on `[[output]]`. |

Slices that exceed the output bounds are skipped and recorded as `slice-out-of-bounds` in `lastRun.toml`.

---

## Units

Dimension strings accept a numeric value followed by an optional unit suffix (case-insensitive). Default unit is `px`.

| Suffix | Meaning |
|---|---|
| `px` | Pixels (literal). |
| `vw` | 1 % of viewport width. |
| `vh` | 1 % of viewport height. |
| `vmin` | 1 % of `min(width, height)`. |
| `vmax` | 1 % of `max(width, height)`. |

The "viewport" is the output for non-sliced rendering and the slice for sliced rendering.

## colors

| Format | Example |
|---|---|
| 6-digit hex | `#FF8800` |
| 8-digit hex with alpha | `#FF8800CC` |
| `rgb(r, g, b)` | `rgb(255, 136, 0)` (channels 0–255) |
| `rgba(r, g, b, a)` | `rgba(255, 136, 0, 0.8)` (alpha 0–1) |
| `hsl(h, s%, l%)` | `hsl(30, 100%, 50%)` |
| `hsla(h, s%, l%, a)` | `hsla(30, 100%, 50%, 0.8)` |
| keyword | `transparent` |

Parsing is invariant-culture; channel values are clamped.

## Substitution tokens

These tokens are expanded inside text and `logo.source` values:

| Token | Value |
|---|---|
| `${MachineName}` | `Environment.MachineName` |
| `${Width}` | Output width in pixels (or slice width in slice scope). |
| `${Height}` | Output height in pixels (or slice height in slice scope). |
| `${Index}` | Zero-based output index. |
| `${IndexPlusOne}` | Output index + 1. |
| `${OutputName}` | `OutputRecord.FriendlyName`. |
| `${ParentIndex}` | Output index (slice scope only). |
