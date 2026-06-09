# Troubleshooting

When BgRaster doesn't do what you expect, the first place to look is the run's stdout (every event is prefixed `# bg-raster: status=...`) and the second is `lastRun.toml` (or `lastRun.dry.toml`) in the output directory. Both are intended to be greppable and human-readable.

---

## "Nothing happened" — `run-skipped-unchanged`

If you see `# bg-raster: status=run-skipped-unchanged` and exit code 0 with no PNGs being written, the early-exit fingerprint matched the previous run.

**The fingerprint is three things, all of which must match `lastRun.toml`:**
- assembly informational version
- settings hash (computed from your effective config)
- hardware profile (output count, IDs, position, resolution, rotation, DPI)

**If you want to force a re-render**, do one of:
- delete `lastRun.toml` from the output directory;
- pass `--no-assignment true` (the no-assignment path uses `lastRun.dry.toml` and is treated as a separate fingerprint, but a no-assignment never assigns wallpaper);
- change any setting (even adding a trailing comment to a string value will flip the hash);
- run with `--render-output <some-other-path>` to bypass the existing `lastRun.toml`.

The early-exit only triggers when **not** in no-assignment mode. Dry runs always render.

---

## Wrong PNG resolution / blurry wallpaper

The PNG is sized to the output's **physical** pixel dimensions, queried via `EnumDisplaySettingsExW` and `GetDpiForMonitor`. If the resolution looks wrong:

1. Check the `[[hardware_output]]` block in `lastRun.toml` — `widthPx`, `heightPx`, `dpiX`, `dpiY` are recorded as observed.
2. Confirm the BgRaster.exe manifest is `PerMonitorV2`-aware. If you've replaced the manifest or are running an unsigned rebuild, mixed-DPI desktops can report logical (DIP) instead of physical pixels.
3. Verify you are running in a normal interactive session. Some remote, service, or locked-down sessions can restrict graphics/wallpaper APIs.

---

## Wallpaper didn't apply

The most common causes:

- **Per-monitor wallpaper unsupported.** Older Windows builds (pre-1809) lack the necessary COM contract. Verify Windows version.
- **Wallpaper slideshow active.** Windows Personalization "slideshow" mode periodically overwrites per-monitor assignments. Disable it.
- **Group Policy lockdown.** "Prevent changing desktop background" blocks the assignment. Look for `wallpaper-assignment-failed` in stdout — the run will exit 1.
- **Stretched / spanned wallpaper mode.** Even if `SetWallpaper` succeeds, Windows can render the per-monitor PNG stretched. Set `IDesktopWallpaper::SetPosition` to `DWPOS_FILL` from Personalisation, or right-click desktop → Personalize → Background → Picture position → "Fill".

If `lastRun.toml` shows `output-rendered` but the wallpaper visibly didn't change, the failure is downstream of BgRaster — check the points above.

---

## "Output-not-found" — configured target doesn't match a real display

If `lastRun.toml` reports `output-not-found target=...` for a configured `[[output]]`:

- **Integer target** (`target = 0`) is matched by zero-based index in the order returned by `EnumDisplayDevicesW`. Reordering displays in Windows Settings *can* change this index.
- **String target** (`target = "\\\\?\\DISPLAY#..."`) must be an exact match for the `OutputRecord.Id` in `lastRun.toml`'s `[[hardware_output]]` block. Copy-paste it from there into your config to be sure.
- The full `Id` includes adapter / monitor instance GUIDs. If you replaced the monitor (even with an identical model) or moved it to a different DisplayPort, the `Id` will change.

Run once with no `[[output]]` entries to capture all hardware outputs in `lastRun.toml`, then copy IDs into your config.

---

## Duplicate-output-ignored

Two `[[output]]` entries claim the same hardware output. The first wins; the second emits `duplicate-output-ignored`. Check whether you have:

- two integer targets that resolve to the same index (unlikely);
- an integer target and a string target that both resolve to the same physical output (likely);
- multiple identical string targets.

Remove or re-target the redundant entry.

---

## Slice rendered out of bounds

`slice-out-of-bounds reason="slice rect (x=...,y=...,w=...,h=...) exceeds output bounds (WxH)"` means the resolved slice rectangle (after unit resolution) extends beyond the output. The slice is silently skipped.

Common causes:

- Mixing `vw` / `vh` against an output you assumed was 1920×1080 but is actually rotated to portrait (1080×1920).
- Rounding from `vmin`/`vmax` units producing 1-pixel overhang at certain resolutions.
- `x + width > vw` once both are resolved to pixels.

Adjust dimensions to fit, or use `vw`/`vh` instead of `px` so the slice scales with the output.

---

## Logo doesn't render / orange cross appears instead

The orange diagonal cross is the ultimate fallback. If you're seeing it instead of your logo:

- **PNG/JPG**: confirm the path is correct and the file is readable. Substitution tokens (`${MachineName}`, etc.) are applied to `logo.source`, so a malformed token will produce a missing-file path.
- **SVG**: BgRaster's SVG renderer supports a deliberately small subset — `<rect>`, `<line>`, `<path>` (M/m/L/l/H/h/V/v/Z only), plus `fill`, `stroke`, `stroke-width`, `opacity`. Curves (C/Q/A/S/T) are not yet implemented, and unrecognised elements are skipped. Re-author the SVG with straight segments, or convert to PNG.
- The first fallback before the orange cross is the embedded `resources/fallback-logo.svg`. If you also see the orange cross, the embedded fallback failed to render too — this should never happen and indicates an SkiaSharp issue worth reporting.

---

## Stale files accumulating in the output directory

`StaleFileCleaner.RecycleFiles` is currently a stub: it identifies stale BgRaster PNGs but does not yet move them to the recycle bin (see [deferred task 7](developer/future-plans.md)). They remain on disk and are reported as `unrecycledFiles` in `lastRun.toml`. You can safely delete them manually.

The "stale file" heuristic only matches files whose names follow the BgRaster timestamp pattern (`yyyy-MM-ddTHH-mm-ss.fffffffZ_<id>.png`); other files in the output directory are left alone.

---

## "Round-trip verification failed"

If you see `LastRunWriter: round-trip verification failed for '...'; previous file kept.`, the writer produced TOML that did not parse back to the same in-memory state. The previous `lastRun.toml` is preserved untouched, so this is a diagnostic — the run otherwise completed.

This indicates a serialisation bug. To capture diagnostics:

1. Re-run with `--render-output <fresh-dir>` to isolate state.
2. Inspect the leftover `<path>.tmp` file (if it survived) against the in-memory expectation.
3. File a bug report with both the `.tmp` content and the previous `lastRun.toml`.

---

## "Output-discovered" listed for outputs I don't care about

`output-discovered` means BgRaster saw a hardware output that had no matching `[[output]]` configuration and skipped it. This happens when `outputs-skip-unspecified = true` (or `--outputs-skip-unspecified true`).

Default behavior is now to render discovered-but-unspecified outputs using global defaults. If you want the previous behavior (leave unspecified outputs untouched), enable `outputs-skip-unspecified`.

---

## Performance / startup latency

BgRaster is Native AOT and startup should be sub-100 ms even on cold cache. If you're seeing slow runs:

- **First run after publish**: file system metadata caching can add a few hundred ms. Subsequent runs should be fast.
- **`alternating = true`**: this fills the bitmap pixel-by-pixel and is O(W×H). For 4K outputs this is a few hundred ms by itself.
- **Many outputs with high resolution**: each output is rendered serially. A 6×4K wall takes proportionally longer.
- **PNG encode**: at 4K, encoding alone can take 100–300 ms per output depending on disk speed.

If you really need parallelism, the renderer is structured to allow it but currently runs serially for simplicity and predictable temp-file paths.
