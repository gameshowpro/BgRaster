# Documentation Generator

## Purpose

Sample configs in [sample-config](sample-config/background-image.toml) are rendered into PNG artifacts in [generated](generated/background-image.png). Each config demonstrates one visual feature and is designed for stable documentation references.

Generated sections in CLI/TOML reference pages are written to `docs/generated/*.md` by `scripts/generate-documentation.ps1`, and runtime CLI metadata is generated into C# at build time from schema metadata. The schema is the single source of truth for TOML defaults/descriptions, CLI help metadata, and documentation tables.

## Current state

1. Schema metadata in `docs/schemas/bgraster-config.schema.json` is the source of truth for TOML and CLI surfaces.
1. Build-time generation emits strongly-typed C# option metadata from schema.
1. Runtime consumes generated C# metadata only and does not parse schema files.
1. Docs generation reads the same schema metadata contract and emits markdown tables under `docs/generated`.
1. Static pages include generated sections using `pymdownx.snippets`.

This removes duplicate hand-maintained metadata between schema and runtime code.

## Metadata model

The schema uses a namespaced extension key: `x-bgraster`.

Option metadata includes:

1. Stable key (for deterministic ordering and diff stability)
1. CLI alias and value syntax
1. Parse kind/type hint
1. Surface visibility (`toml`, `cli`, or `both`)
1. TOML mapping path (when applicable)
1. Help/description and default-resolution text

CLI-only options (for example `--config`) live under `cliOnlyOptions` in the same extension.

## How generation works

Sample asset generation (`scripts/generate-documentation.ps1`):

1. Enumerates all TOML files in `docs/sample-config`.
1. Invokes BgRaster once per file.
1. Forces non-assignment mode (`--no-discovery true --no-assignment true`).
1. Uses `--render-output` with a deterministic stem derived from the sample file name.
1. Writes PNG outputs to `docs/generated`.

Metadata generation pipeline:

1. Build uses an incremental source generator to read schema metadata and emit C# option metadata.
1. CLI binding/help consumes generated metadata for option descriptions and mappings.
1. `scripts/generate-documentation.ps1` reads schema metadata and writes generated markdown files under `docs/generated/` before `mkdocs build`/`mkdocs serve` runs.
1. Pages publish fully expanded tables via snippet includes in [cli-schema.md](cli-schema.md) and [toml-schema.md](toml-schema.md).

## Implementation details

The project uses a Roslyn Incremental Source Generator.

Rationale:

1. Modern .NET pattern for compile-time metadata projection
1. Incremental and cached for fast rebuilds
1. Zero runtime schema IO or JSON parse cost
1. Strongly typed generated data with compile-time failures on invalid metadata
1. Easy parity testing between schema metadata, generated code, and docs outputs

## Config requirements for samples

Each sample config must:

1. Use `[render].no-discovery = true`.
1. Include one `[[output]]` entry with `[output.hardware_output]` dimensions.
1. Disable unrelated layers where possible so each sample demonstrates a single feature.

Current fixed hardware profile used for simple samples:

```toml
index = 0
desktopX = 0
desktopY = 0
widthPx = 640
heightPx = 480
dpiX = 96
dpiY = 96
rotation = 0
adapterName = "FIXED"
friendlyName = "Unspecified"
```

## Path behavior

Sample configs intentionally use relative paths (for example, `images/logo.png`). Path handling rules are documented centrally in [toml-schema.md](toml-schema.md#path-resolution).