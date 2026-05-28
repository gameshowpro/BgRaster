# Deferred Tasks

These tasks require deep domain expertise or involve complex Windows interop, and are best assigned to a capable LLM in a follow-up pass. Each entry describes the task, where the stub lives, what "done" looks like, and the key constraints.

---

## 1. Windows Display Discovery — ✅ COMPLETED

Implemented in `src/Discovery/DisplayDiscovery.cs`. Uses `EnumDisplayDevicesW` (with `EDD_GET_DEVICE_INTERFACE_NAME`) for adapter/monitor enumeration, `EnumDisplaySettingsExW` for `DEVMODE` (position/size/rotation/refresh), `MonitorFromPoint` + `GetDpiForMonitor` for per-monitor DPI, and `QueryDisplayConfig` + `DisplayConfigGetDeviceInfo` (with `DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME`) for `monitorDevicePath` and `monitorFriendlyDeviceName`. AOT-safe via `LibraryImport` and `unsafe fixed char[]` buffers in lieu of `MarshalAs(ByValTStr)`.

<details><summary>Original task spec</summary>

**Stub:** `src/Discovery/DisplayDiscovery.cs`

**Problem:** Correlating `EnumDisplayDevices` (which gives GDI device names like `\\.\DISPLAY1`) with `DisplayConfigGetDeviceInfo` (which gives the `monitorDevicePath` used by `IDesktopWallpaper`) is non-trivial. GDI and Display Config APIs use different identifiers and require careful correlation via adapter/source indexes.

**Done when:**
- `DisplayDiscovery.Discover()` returns a real `HardwareProfile` built from the active physical displays
- Each `OutputRecord` has the correct `monitorDevicePath` (the `\\?\DISPLAY#...` form expected by `IDesktopWallpaper::SetWallpaper`)
- `FriendlyName` is populated from `DisplayConfigGetDeviceInfo` with `DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME`
- DPI is read from `GetDpiForMonitor` (the `HWND` can be obtained via `MonitorFromPoint` for the monitor's desktop origin)
- Rotation is populated from `DEVMODE.dmDisplayOrientation`
- `DesktopX/Y` and `WidthPx/HeightPx` are populated from `DEVMODE.dmPosition` and `dmPelsWidth/dmPelsHeight`
- Works correctly on multi-adapter configurations (e.g., NVIDIA + Intel iGPU)

**Key constraints:**
- Native AOT: use `LibraryImport` (source-generated P/Invoke), not `DllImport`
- `StringMarshalling.Utf16` for all wide-char (`W`-suffix) API calls
- `unsafe` blocks and `fixed` may be required for struct pinning
- Existing signatures in `src/Discovery/Interop/DisplayInterop.cs` and struct definitions in `src/Discovery/Interop/NativeStructs.cs` should be used as the starting point; extend as needed

---

- Existing signatures in `src/Discovery/Interop/DisplayInterop.cs` and struct definitions in `src/Discovery/Interop/NativeStructs.cs` should be used as the starting point; extend as needed

</details>

---

## 2. COM `IDesktopWallpaper` Wallpaper Assignment — ✅ COMPLETED

Implemented in `src/Wallpaper/WallpaperAssigner.cs`. `CoInitializeEx` + `CoCreateInstance(CLSID_DesktopWallpaper, CLSCTX_LOCAL_SERVER)` obtains the COM instance; `SetWallpaper`/`Release` are invoked via manual vtable lookup using `delegate* unmanaged[Stdcall]<...>` function pointers. `AssignAsync` calls `SetWallpaper` per output; `ClearAsync` calls it with an empty path. Per-monitor failures are logged and do not abort the run. Fully AOT-safe (no `[ComImport]`).

<details><summary>Original task spec</summary>

**Stub:** `src/Wallpaper/WallpaperAssigner.cs`

**Problem:** `IDesktopWallpaper` is a COM interface with no generated interop in .NET 10 Native AOT. The vtable must be called manually via unsafe pointer arithmetic, or via a hand-written COM callable wrapper.

**Done when:**
- `WallpaperAssigner.AssignAsync` calls `IDesktopWallpaper::SetWallpaper(monitorId, imagePath)` for each output
- `WallpaperAssigner.ClearAsync` calls `IDesktopWallpaper::SetWallpaper(monitorId, "")` for the given set of monitor IDs
- The implementation handles `CoCreateInstance` correctly for `CLSID_DesktopWallpaper`
- Errors (e.g., monitor not found, path invalid) are logged and do not crash the process
- Works in an AOT-published binary (no reflection-based COM marshalling)

**Key constraints:**
- GUIDs are defined in `src/Wallpaper/Interop/WallpaperInterop.cs`
- The `monitorId` passed to `SetWallpaper` must be the `monitorDevicePath` (`\\?\DISPLAY#...` form) — this is the string returned by `IDesktopWallpaper::GetMonitorDevicePathAt`, which must match the `OutputRecord.Id` populated by task 1 above
- Consider using `LibraryImport("ole32.dll")` for `CoCreateInstance` and manual vtable offset calls (vtable index 7 for `SetWallpaper`, 8 for `GetWallpaper`, per IDesktopWallpaper documentation)
- AOT constraint: no `[ComImport]` / `[Guid]` attribute-based COM interop — those require runtime marshalling

</details>

---

## 3. Grid Coordinate Overlay — ✅ COMPLETED

Implemented in `src/Rendering/Layers/GridLayer.cs`. When `ResolvedOptions.GridCoordinates` is `true`, each cell renders a column number (top-left, after a corner triangle), an `x` divider near center, and a row number (bottom-right). Text/triangle color is selected per-cell from the cell's odd/even color luminance (0.299·R + 0.587·G + 0.114·B with alpha attenuation; <0.5 → white text/triangle, else black). Font is `FontManager.Typeface` at `clamp(cellSize·0.22, 6, 18)` px; cells <12px skip overlay. Paints/font are created once per render call.

<details><summary>Original task spec</summary>

**Stub:** `src/Rendering/Layers/GridLayer.cs` (TODO comment near bottom of `Render`)

**Problem:** Each grid cell should optionally render its (col, row) coordinate as a small label. The text color must have sufficient luminance contrast against the cell background color (WCAG AA: ≥4.5:1 ratio). A small triangle in one corner of the cell should indicate the cell's parity.

**Done when:**
- When `ResolvedOptions.GridCoordinates` is `true`, each cell renders its `(col, row)` label centered in the cell
- Text color is chosen automatically: measure the cell's background luminance (relative luminance formula per WCAG 2.1), then pick white or black for ≥4.5:1 contrast ratio
- A small right-triangle (≤8px per side) is rendered in the top-left corner of even cells using the OddColor for contrast
- Font is `FontManager.Typeface` at a size proportional to `GridSizePx` (e.g., `GridSizePx * 0.25f`, clamped to [8, 16])
- Text does not overflow the cell boundary; truncate or skip if cell is too small

**Key constraints:**
- `ResolvedOptions` already has `GridCoordinates bool` — add it to `ResolvedOptions.cs` and `OptionsResolver.cs` if missing
- Use `SKCanvas.DrawText` with `SKFont` from `FontManager`; do not use deprecated `SKPaint.TextSize` path
- Performance: create `SKFont` and `SKPaint` once outside the cell loop, not per-cell

---

- Performance: create `SKFont` and `SKPaint` once outside the cell loop, not per-cell

</details>

---

## 4. SVG Logo Rendering — ✅ COMPLETED

Implemented in `src/Rendering/SvgRenderer.cs` (parser + renderer) and wired into `src/Rendering/Layers/LogoLayer.cs`. Uses `System.Xml.XmlReader` (AOT-safe; no reflection deserialization). Supports `<svg viewBox>` (with width/height fallback), `<rect>`, `<line>`, and `<path>` (M/m/L/l/H/h/V/v/Z absolute and relative commands — no curves yet). Honours `fill`, `stroke`, `stroke-width`, `opacity`, plus `none`. Document is scaled into the logo rect via BestFit; `LogoOpacity` multiplies into per-shape alpha. The embedded `resources/fallback-logo.svg` is now used as the primary fallback when an explicit SVG fails to parse, with the original programmatic orange-cross as the ultimate last resort. Curves (`C/c/Q/q/A/a/S/s/T/t`) are documented as out of scope for the minimum viable implementation.

<details><summary>Original task spec</summary>

**Stub:** `src/Rendering/Layers/LogoLayer.cs` (the `IsSvg` branch falls through to the fallback cross)

**Problem:** SkiaSharp's `SKSvg` is in a separate NuGet package (`SkiaSharp.Svg`) and has AOT/trim compatibility issues. An alternative is to use a pure-managed SVG renderer or parse the SVG subset used by the fallback logo directly.

**Done when:**
- When `LogoSource` ends with `.svg` (case-insensitive), the SVG is loaded and rendered into the logo rect
- The fallback cross SVG embedded as `resources/fallback-logo.svg` renders correctly when `LogoSource` is empty
- SVG viewport is scaled to fit the logo rect using `BestFit` logic (preserves aspect ratio)
- `LogoOpacity` is applied as an alpha modifier
- If the SVG fails to load or parse, the programmatic orange cross fallback is used (existing behavior)

**Key constraints:**
- Must be AOT-safe: no reflection-based XML deserialization; either use `System.Xml.XmlReader` directly or a vetted AOT-compatible SVG library
- Do not add `SkiaSharp.Svg` unless its AOT/trim compatibility has been verified — see deferred task 9 for this validation
- Minimum viable scope: handle `<svg>`, `<rect>`, `<line>`, `<path>` (straight-line segments only), `stroke`, `fill`, `opacity` attributes — sufficient for the fallback logo

**Alternatives**
A last resort would be allowing an pre-build native svg decoder to be placed on the path (e.g. in next to BgRaster.exe) so that the SVG can be converted to a raster format at the requested resolution that BgRaster.exe can decode.

</details>
---

## 5. `lastRun.toml` Generated Status Comments — ✅ COMPLETED

Implemented in `src/StateCache/LastRunWriter.cs` together with `src/Models/RunStatus.cs` and the per-output status flow in `src/Program.cs`. Status is tracked in `RunStatus` (per-hardware) and `ConfiguredOutputStatus` (per `[[output]]` and per `[[output.slice]]`). The writer emits `# bg-raster: status=<value> ...` comments before each `[[hardware_output]]`, `[[output]]`, and `[[output.slice]]` block. Status values: `output-rendered` / `output-discovered` (hardware), `output-matched` / `output-not-found` / `duplicate-output-ignored` (configured outputs), `slice-rendered` / `slice-out-of-bounds` (slices). Hand-rolled TOML emission was used rather than Tomlyn's document model (Tomlyn's comment API in 0.17.x is fragile for header comments on `[[array_of_tables]]`).

<details><summary>Original task spec</summary>

**Stub:** `src/StateCache/LastRunWriter.cs` (TODO comment above each section)

**Problem:** The `plan.md` spec requires that each output entry in `lastRun.toml` include an inline comment showing its assignment status (e.g., `# assigned: wallpaper.png`, `# skipped: no match`). Tomlyn's document model does not emit comments on value nodes by default.

**Done when:**
- Each `[[hardware_output]]` entry in the written TOML has a trailing `# status: <value>` comment on the same line as the `id` key
- The status values match those defined in `plan.md` (check the spec for the exact enumeration)
- Comments survive a read-then-write round-trip without duplication or corruption

**Key constraints:**
- Tomlyn's `TomlTable`/`TomlValue` types do support attaching comments via the `Comment` property on `TomlObject` — explore this API before hand-rolling string concatenation
- If Tomlyn cannot reliably attach comments, fall back to writing the TOML as formatted string output with manual comment injection after the key

</details>

---

## 6. `lastRun.toml` Round-Trip Write Verification — ✅ COMPLETED

Implemented in `src/StateCache/LastRunWriter.cs`. The writer now writes to `path + ".tmp"`, calls `LastRunReader.Read` to parse the temp file, then compares meta scalars, `AssignedFiles` (via `DictionaryEqual`), `UnrecycledFiles` (via `SequenceEqual` with `StringComparer.Ordinal`), and the sorted hardware records by record value equality. On success, `File.Move(tempPath, path, overwrite: true)` performs an atomic replace. On mismatch, the temp file is deleted and the previous `lastRun.toml` is preserved; a diagnostic message is logged via `Console.WriteLine`. Mismatches do not throw or fail the run.

<details><summary>Original task spec</summary>

**Stub:** `src/StateCache/LastRunWriter.cs` (TODO comment at end of `Write`)

**Problem:** After writing `lastRun.toml`, the file should be read back and verified to match the in-memory `LastRunState` that was written, to catch serialization bugs early.

**Done when:**
- After a successful write, `LastRunReader.Read(path)` is called and the resulting `LastRunState` is compared field-by-field against the original
- If any field mismatches, a warning is logged with the mismatching key and both values; the file is left in place (do not delete on mismatch — the user may want to inspect it)
- The comparison is a value equality check (not reference), using `ImmutableArray.SequenceEqual` for arrays and `FrozenDictionary.SequenceEqual` for dictionaries

**Key constraints:**
- This is a diagnostic check, not a correctness gate — a mismatch should log at `Warning` level, not throw
- `LastRunState` and its nested types already implement value equality as `record` types; `ImmutableArray<T>` does not — use `SequenceEqual` explicitly

</details>

---

## 7. Windows Shell Recycle Bin (`IFileOperation`)

**Stub:** `src/FileLifecycle/StaleFileCleaner.cs` — `RecycleFiles` method

**Problem:** Moving files to the recycle bin requires `IFileOperation` COM interop, which has the same AOT challenges as `IDesktopWallpaper` (task 2).

**Done when:**
- `RecycleFiles(ImmutableArray<string> paths)` sends each file to the recycle bin using `IFileOperation::DeleteItem` with `FOF_ALLOWUNDO`
- Returns an `ImmutableArray<string>` of paths that failed to recycle (empty on full success)
- On failure, the path is logged and included in the returned "unrecycled" array (existing contract)
- Works in an AOT-published binary

**Key constraints:**
- `IFileOperation` CLSID is `{3ad05575-8857-4850-9277-11b85bdb8e09}`; `IID_IFileOperation` is `{947aab5f-0a5c-4c13-b4d6-4bf7836b9f59}`
- Same AOT vtable-calling approach required as task 2 — consider implementing a shared COM helper to reduce duplication
- The `FOF_ALLOWUNDO` flag (0x40) is required for recycle-bin behavior; without it, the file is permanently deleted

**Alternatives**
If it no path to deleting to the recycle bin can be established that is compatible with AOT, this requirement should be placed on the "for future development" list of the documentation and abandoned for now.
---

## 8. Golden Image Regression Tests

**Problem:** The rendering layers have no pixel-level regression tests. Visual regressions (e.g., wrong color, misaligned text, broken FitMode) are undetectable without them.

**Done when:**
- A test fixture renders each layer in isolation against a fixed synthetic `OutputRecord` (1920×1080) and compares the output PNG against a committed reference image
- A tolerance of ±1 per channel per pixel is accepted (font rasterization and platform-specific float rounding may differ slightly)
- Tests cover: `BackgroundLayer` (solid fill, each `FitMode`), `GridLayer`, `BorderLayer`, `CircleLayer`, `CrosshairLayer`, `TextLayer` (title + subtitle)
- A helper script generates/updates the reference images and commits them to `tests/BgRaster.Tests/ReferenceImages/`

**Key constraints:**
- Tests must run on `windows-latest` GitHub Actions runners — font rendering may differ on Linux; do not add Linux runner for these tests
- Use `SkiaSharp.SKBitmap` for pixel comparison (already a test dependency)
- Reference images are binary-committed to the repo; use `.gitattributes` to mark them as binary to avoid line-ending corruption

---

## 9. `DisableRuntimeMarshalling` + SkiaSharp AOT Compatibility Validation

**Problem:** `DisableRuntimeMarshalling=true` is the recommended setting for minimal AOT binary size, but SkiaSharp's native interop may rely on runtime marshalling for some code paths (particularly `SKBitmap.GetPixels()`, `SKCanvas.DrawBitmap`, and font loading). This has not been validated.

**Done when:**
- A publish matrix in CI tests `DisableRuntimeMarshalling=true` vs `false`
- All existing integration tests pass with `DisableRuntimeMarshalling=true` on a real AOT binary
- If any SkiaSharp API is incompatible, the incompatible call site is documented here with the specific exception or linker error
- If fully compatible, `DisableRuntimeMarshalling=true` is added to `BgRaster.csproj` and this task is closed

**Key constraints:**
- Test against SkiaSharp 2.88.x (pinned version in csproj)
- The `AlternatingLayer` uses `GetPixels()` with unsafe pointer access — this is the most likely breakage point
- Do not enable `DisableRuntimeMarshalling` speculatively; wait for this validation to complete
