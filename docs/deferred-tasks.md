# Deferred Tasks

This document tracks the remaining work items and the current understanding of what is already implemented.

## 1. Recycle existing behavior into the new display pipeline - DONE

Done:
- Display discovery is handled through the current `DisplayDiscovery` path.
- The resolver and renderer pipeline now work from the discovered display geometry.
- The task is reflected in the production code instead of remaining a backlog-only note.

## 2. Recycle Bin support for stale outputs - PARTIAL

Done:
- Stale `.png` files are detected with `StaleFileCleaner.FindStaleFiles`.
- Stale file intent is logged so the retry path remains visible.

Not done:
- The Windows shell recycle-bin integration is still stubbed.
- `IFileOperation` / COM-backed recycle behavior is not implemented yet.
- Stale files are returned to the caller for retry on the next run.

## 3. Golden image regression coverage - PARTIAL

Done:
- The renderer is stable enough to support pixel comparison tests.
- The codebase already has focused unit tests around parsing, resolution, and hashing.

Not done:
- Reference image fixtures are not yet established.
- There is no golden comparison harness for rendered outputs.
- There is no tolerance or diff-report workflow for visual regressions.

## 4. Native AOT validation for rendering dependencies - PARTIAL

Done:
- The codebase has already been pushed toward AOT-friendly patterns.
- SkiaSharp usage is isolated behind rendering layers and helper types.

Not done:
- `DisableRuntimeMarshalling` has not been enabled globally.
- The publish/test matrix against the pinned SkiaSharp version still needs explicit validation.
- The unsafe pixel-access path in `AlternatingLayer` remains the most likely breakage point.

## 5. Labeled edges feature - DONE

This feature adds arrows to slice edges or corners to call out pixel numbers relative to the output. The arrow tip lands on the chosen side, and the label is anchored from the corresponding corner so the visual callout is stable.

Done:
- Config model and overrides exist for labeled edges.
- The TOML loader validates `side` values and rejects duplicates.
- The renderer now has a dedicated labeled-edges layer with arrow and label drawing.
- Schema, sample config, and docs were updated to include the new feature.
- Tests cover parsing, resolution, geometry, and rendering.

## 6. Seed config generation after successful execution - DONE

Done:
- Missing explicit config files are seeded only after a successful run.
- The generated seed config uses the current schema URL and the current default configuration shape.
- The seed path is separate from `lastRun.toml` persistence.

## 7. Config parse failure handling - DONE

Done:
- TOML parse failures are wrapped with file context.
- Config loading now reports a standardized error message instead of crashing with an unhandled exception.
- The exit path is consistent with a normal configuration error.

## 8. Strict substitution tokens - DONE

Done:
- The runtime substitution model now uses the strict `Output*` and `Slice*` token set.
- Legacy alias tokens were removed to keep the configuration language explicit.
- The docs and samples were updated to match the current token names.

## 9. Schema and sample-config alignment - DONE

Done:
- The generated sample config matches the strict schema.
- Schema references in TOML comments now point at the published schema location.
- The sample config reflects the actual precedence and default behavior used by the resolver.

## 10. Labeled edges feature implementation plan - DONE

This is the implementation breakdown for the labeled edges work. The feature is now present in code, so this section documents what was delivered.

Done:
- The desired behavior was clarified into a render step similar to the other overlay layers.
- The feature now has a config model, validation, resolver wiring, and a render layer.
- The feature is documented in schema and sample config.

Not done:
- No remaining work is tracked for this task.
