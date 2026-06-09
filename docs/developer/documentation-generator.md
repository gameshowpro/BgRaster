# Documentation Generator

## Purpose

Sample configs in [sample-config](../sample-config/background-image.toml) are rendered into PNG artifacts in [generated](../generated/background-image.png). Each config demonstrates one visual feature and is designed for stable documentation references.

Generated sections in CLI/TOML reference pages are written to `docs/generated/*.md` by `scripts/generate-documentation.ps1` from schema metadata in `docs/schemas/bgraster-config.schema.json`, then included from the static pages with `pymdownx.snippets` so the docs stay aligned with source-of-truth defaults and descriptions.

## How generation works

The generator script (`scripts/generate-documentation.ps1`):

1. Enumerates all TOML files in `docs/sample-config`.
1. Invokes BgRaster once per file.
1. Forces non-assignment mode (`--no-discovery true --no-assignment true`).
1. Uses `--render-output` with a deterministic stem derived from the sample file name.
1. Writes PNG outputs to `docs/generated`.

The generator script (`scripts/generate-documentation.ps1`):

1. Reads `x-cli-options` from `docs/schemas/bgraster-config.schema.json`.
1. Writes generated markdown files under `docs/generated/` before `mkdocs build`/`mkdocs serve` runs.
1. Keeps source files stable in git while still publishing fully expanded tables via snippet includes in [cli-schema.md](../cli-schema.md) and [toml-schema.md](../toml-schema.md).

## Config requirements for samples

Each sample config should:

1. Use `[render].no-discovery = true`.
1. Include one `[[output]]` entry with `[output.hardware_output]` dimensions.
1. Disable unrelated layers where possible so each sample demonstrates a single feature.

Recommended fixed hardware profile for simple samples:

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

Sample configs intentionally use relative paths (for example, `images/logo.png`). Path handling rules are documented centrally in [toml-schema.md](../toml-schema.md#path-resolution).