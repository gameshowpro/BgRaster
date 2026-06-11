# TOML Schema Reference

BgRaster reads its primary configuration from a TOML file. If `--config` is omitted, it searches for `config.toml` in this order: next to the executable, `%ProgramData%\BgRaster`, `%LocalAppData%\BgRaster`, and `%AppData%\BgRaster`. If `--config` is provided and the file does not exist, BgRaster uses built-in defaults for that run; after a successful execution, it writes a seeded `config.toml` template at the requested path using effective global defaults and detected output targets. Every key is optional — missing keys fall back to the documented defaults. CLI flags override the corresponding TOML values; per-output overrides take precedence over globals; per-slice overrides take precedence over per-output.

## Conventions

- **All multi-value globals are arrays.** `BgRaster` cycles through array elements per output index using `array[index % array.Length]`. Specifying a single-element array applies the value to every output.
- **Dimension strings** accept the units listed under [Units](#units).
- **color strings** accept the formats listed under [colors](#colors).
- **Field substitution** is applied to text and path-bearing values — see [Path resolution](#path-resolution) and [Substitution tokens](#substitution-tokens).
- **TOML keys use kebab-case** (e.g. `border-color`, `grid-coordinates`); the C# model uses PascalCase (`BorderColor`, `GridCoordinates`).

## Root scalars

Top-level scalar values (outside section tables):

<!-- BEGIN:TOML_ROOT_SCALARS_TABLE -->
--8<-- "generated/toml-root-scalars.md"
<!-- END:TOML_ROOT_SCALARS_TABLE -->

<!--
| Key | Type | Default | Description |
|---|---|---|---|
| `machine-name` | `string` | `""` (uses framework host name) | Override the framework-supplied host name used for `${MachineName}` substitutions across text and path-bearing properties. |
-->

## Path resolution

The following properties share the same path resolution behavior:

- `[background].image`
- `[logo].source`
- `[render].output`
- `[[output]].background.image`
- `[[output]].logo.source`
- `[[output.slice]].background.image`
- `[[output.slice]].logo.source`

Resolution order:

1. Field substitution is applied first (see [Substitution tokens](#substitution-tokens)).
2. Environment variables are expanded.
3. If the value is an absolute URI (including `pack://`), it is used as-is.
4. If the value is an absolute filesystem path, it is used as-is.
5. Otherwise, it is treated as a relative path.

Relative-path base directory:

- For TOML-sourced values, the path is resolved against the directory of the TOML file that contains that relative path.
- For CLI-provided values (for example `--background-image`, `--logo-source`, `--render-output`), the path is resolved against the current working directory.

Notes:

- Empty values remain empty.
- Property-specific semantics still apply after resolution (for example, `[background].image` empty disables background image rendering, and `[logo].source` empty suppresses logo rendering).

<!-- BEGIN:TOML_SCHEMA_SECTIONS -->
--8<-- "generated/toml-schema-sections.md"
<!-- END:TOML_SCHEMA_SECTIONS -->
