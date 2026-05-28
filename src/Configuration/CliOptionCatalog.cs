namespace GameshowPro.BgRaster.Configuration;

internal sealed record CliOptionDefinition(
    string Alias,
    string? ValueSyntax,
    string TypeName,
    string TomlEquivalent,
    string Description,
    string DefaultResolution)
{
    internal string OptionSyntax => ValueSyntax is null ? Alias : $"{Alias} {ValueSyntax}";

    internal string HelpDescription => $"{Description} Default resolution: {DefaultResolution}";
}

static class CliOptionCatalog
{
    internal const string OptionsTableBeginMarker = "<!-- BEGIN:CLI_OPTIONS_TABLE -->";
    internal const string OptionsTableEndMarker = "<!-- END:CLI_OPTIONS_TABLE -->";

    internal static ImmutableArray<CliOptionDefinition> Definitions { get; } =
    [
        new("--config", "<path>", "string", "-", "Path to a TOML config file.", "if omitted, uses config.toml next to the executable."),
        new("--text-title", "<s>", "string", "[text].title", "Override title text.", "if omitted, defers to config.toml [text].title; if missing there, uses [\"${MachineName} ${Index}\"]."),
        new("--text-subtitle", "<s>", "string", "[text].subtitle", "Override subtitle text.", "if omitted, defers to config.toml [text].subtitle; if missing there, uses [\"${Width}x${Height}\"]."),
        new("--text-size", "<dim>", "string", "[text].size", "Font height.", "if omitted, defers to config.toml [text].size; if missing there, uses [\"1vh\"]."),
        new("--text-x", "<dim>", "string", "[text].x", "Anchor X.", "if omitted, defers to config.toml [text].x; if missing there, uses [\"75vw\"]."),
        new("--text-y", "<dim>", "string", "[text].y", "Anchor Y.", "if omitted, defers to config.toml [text].y; if missing there, uses [\"75vh\"]."),
        new("--background-color", "<colour>", "string", "[background].color", "Background fill colour.", "if omitted, defers to config.toml [background].color; if missing there, uses [\"#FF0000\", \"#00FF00\", \"#0000FF\"]."),
        new("--background-image", "<path>", "string", "[background].image", "Path to background bitmap.", "if omitted, defers to config.toml [background].image; if missing there, uses [\"\"] (disabled)."),
        new("--background-fit", "<mode>", "string", "[background].fit", "Background fit mode.", "if omitted, defers to config.toml [background].fit; if missing there, uses [\"CropToFill\"]."),
        new("--background-alternating", "<bool>", "bool", "[background].alternating", "Enable alternating-pixel pattern.", "if omitted, defers to config.toml [background].alternating; if missing there, uses [false]."),
        new("--background-border", "<bool>", "bool", "[background].border", "Enable viewport border.", "if omitted, defers to config.toml [background].border; if missing there, uses [false]."),
        new("--background-border-color", "<colour>", "string", "[background].border-color", "Border colour.", "if omitted, defers to config.toml [background].border-color; if missing there, uses [\"#FFFFFF\"]."),
        new("--grid-size", "<dim>", "string", "[grid].size", "Grid cell side length.", "if omitted, defers to config.toml [grid].size; if missing there, uses [\"100px\"]."),
        new("--grid-odd-color", "<colour>", "string", "[grid].odd-color", "Odd-cell colour.", "if omitted, defers to config.toml [grid].odd-color; if missing there, uses [\"#00000080\"]."),
        new("--grid-even-color", "<colour>", "string", "[grid].even-color", "Even-cell colour.", "if omitted, defers to config.toml [grid].even-color; if missing there, uses [\"transparent\"]."),
        new("--grid-stroke", "<dim>", "string", "[grid].stroke", "Cell stroke width.", "if omitted, defers to config.toml [grid].stroke; if missing there, uses [\"0\"]."),
        new("--grid-offset-x", "<dim>", "string", "[grid].offset-x", "Grid origin X.", "if omitted, defers to config.toml [grid].offset-x; if missing there, uses [\"0\"]."),
        new("--grid-offset-y", "<dim>", "string", "[grid].offset-y", "Grid origin Y.", "if omitted, defers to config.toml [grid].offset-y; if missing there, uses [\"0\"]."),
        new("--grid-coordinates", "<bool>", "bool", "[grid].coordinates", "Enable per-cell coordinate labels.", "if omitted, defers to config.toml [grid].coordinates; if missing there, uses [false]."),
        new("--circle-size", "<dim>", "string", "[circle].size", "Circle diameter.", "if omitted, defers to config.toml [circle].size; if missing there, uses [\"100vmin\"]."),
        new("--circle-color", "<colour>", "string", "[circle].color", "Circle colour.", "if omitted, defers to config.toml [circle].color; if missing there, uses [\"#ffffff40\"]."),
        new("--circle-stroke", "<dim>", "string", "[circle].stroke", "Circle stroke width.", "if omitted, defers to config.toml [circle].stroke; if missing there, uses [\"0\"]."),
        new("--crosshair-length", "<dim>", "string", "[crosshair].length", "Crosshair half-arm length.", "if omitted, defers to config.toml [crosshair].length; if missing there, uses [\"5vmin\"]."),
        new("--crosshair-color", "<colour>", "string", "[crosshair].color", "Crosshair colour.", "if omitted, defers to config.toml [crosshair].color; if missing there, uses [\"#ffffff80\"]."),
        new("--crosshair-stroke", "<dim>", "string", "[crosshair].stroke", "Crosshair stroke width.", "if omitted, defers to config.toml [crosshair].stroke; if missing there, uses [\"2px\"]."),
        new("--logo-source", "<path>", "string", "[logo].source", "Path to logo file (PNG/JPG/SVG).", "if omitted, defers to config.toml [logo].source; if missing there, uses [\"\"] (embedded fallback)."),
        new("--logo-x", "<dim>", "string", "[logo].x", "Logo rect left.", "if omitted, defers to config.toml [logo].x; if missing there, uses [\"75vw\"]."),
        new("--logo-y", "<dim>", "string", "[logo].y", "Logo rect top.", "if omitted, defers to config.toml [logo].y; if missing there, uses [\"25vh\"]."),
        new("--logo-width", "<dim>", "string", "[logo].width", "Logo rect width.", "if omitted, defers to config.toml [logo].width; if missing there, uses [\"15vw\"]."),
        new("--logo-height", "<dim>", "string", "[logo].height", "Logo rect height.", "if omitted, defers to config.toml [logo].height; if missing there, uses [\"15vh\"]."),
        new("--logo-opacity", "<0..1>", "string", "[logo].opacity", "Logo alpha multiplier.", "if omitted, defers to config.toml [logo].opacity; if missing there, uses [\"1\"]."),
        new("--no-assignment", "<bool>", "bool", "[render].no-assignment", "Generate PNGs without assigning wallpaper.", "if omitted, defers to config.toml [render].no-assignment; if missing there, uses false."),
        new("--outputs-skip-unspecified", "<bool>", "bool", "[render].outputs-skip-unspecified", "Skip discovered displays that have no explicit [[output]] target.", "if omitted, defers to config.toml [render].outputs-skip-unspecified; if missing there, uses false."),
        new("--render-output", "<path>", "string", "[render].output", "Output directory for generated PNGs.", "if omitted, defers to config.toml [render].output; if missing there, defaults to %TEMP%/BgRaster."),
        new("--render-force", "<bool>", "bool", "[render].force", "Continue rendering even after emitting run-skipped-unchanged.", "if omitted, defers to config.toml [render].force; if missing there, uses false."),
        new("--verbosity", "<level>", "string", "[render].verbosity", "Logging verbosity: quiet, normal, verbose.", "if omitted, defers to config.toml [render].verbosity; if missing there, uses \"normal\".")
    ];

    internal static CliOptionDefinition GetByAlias(string alias) =>
        Definitions.First(d => string.Equals(d.Alias, alias, StringComparison.Ordinal));

    internal static string BuildOptionsMarkdownTable()
    {
        StringBuilder sb = new();
        sb.AppendLine("| Option | Type | TOML equivalent | Description | Default resolution |");
        sb.AppendLine("|---|---|---|---|---|");

        foreach (CliOptionDefinition definition in Definitions)
        {
            sb.Append("| `");
            sb.Append(definition.OptionSyntax);
            sb.Append("` | `");
            sb.Append(definition.TypeName);
            sb.Append("` | `");
            sb.Append(definition.TomlEquivalent);
            sb.Append("` | ");
            sb.Append(definition.Description);
            sb.Append(" | ");
            sb.Append(definition.DefaultResolution);
            sb.AppendLine(" |");
        }

        return sb.ToString().TrimEnd();
    }
}