The documentation publish pipeline is implemented and runs through MkDocs.

Current behavior:

1. Source content lives under `docs/`.
1. Generated assets and markdown fragments are produced by `scripts/generate-documentation.ps1`.
1. CLI and TOML reference sections include generated files via snippet markers in [cli-schema.md](../cli-schema.md) and [toml-schema.md](../toml-schema.md).
1. The site is built with MkDocs Material and published to GitHub Pages by the workflow at `.github/workflows/github-pages.yml`.

Implementation details:

1. The theme is MkDocs Material with color-scheme behavior set to `auto`.
1. Local preview is supported with `mkdocs serve`.
1. Schema metadata in `docs/schemas/bgraster-config.schema.json` drives generated CLI documentation content.