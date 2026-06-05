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
        new("--config", "<path>", "string", "-", "Path to a TOML config file. If the path does not exist, BgRaster starts from built-in defaults for that run, then after a successful execution writes a seeded config template at that path.", "if omitted, searches for config.toml in this order: next to the executable, %ProgramData%\\BgInfo, %LocalAppData%\\BgInfo, %AppData%\\BgInfo; if none exist, starts from built-in defaults."),
        new("--machine-name", "<name>", "string", "machine-name", "Override the framework-supplied host name used by ${MachineName} substitutions.", "if omitted, uses Environment.MachineName."),
        new("--text", "<s|[\"s1\",\"s2\"]>", "string", "[text].text", "Text line(s); accepts a single string or a TOML string array literal.", "if omitted, defers to config.toml [text].text; if missing there, uses [\"${MachineName} output ${OutputIndexPlusOne}\", \"slice ${SliceLetter}\", \"${SliceWidth}x${SliceHeight}\"]."),
        new("--text-size", "<dim|[\"d1\",\"d2\"]>", "string", "[text].size", "Text line height(s); accepts a single dimension or a TOML string array literal.", "if omitted, defers to config.toml [text].size; if missing there, uses [\"3vh\", \"2vh\", \"4vh\"]."),
        new("--text-color", "<color|[\"c1\",\"c2\"]>", "string", "[text].color", "Text color(s); accepts a single color or a TOML string array literal.", "if omitted, defers to config.toml [text].color; if missing there, uses [\"#fff\"]."),
        new("--text-x", "<dim>", "string", "[text].x", "Anchor X.", "if omitted, defers to config.toml [text].x; if missing there, uses [\"75vw\"]."),
        new("--text-y", "<dim>", "string", "[text].y", "Anchor Y.", "if omitted, defers to config.toml [text].y; if missing there, uses [\"75vh\"]."),
        new("--background-color", "<color>", "string", "[background].color", "Background fill color.", "if omitted, defers to config.toml [background].color; if missing there, uses [\"#FF0000\", \"#00FF00\", \"#0000FF\"]."),
        new("--background-image", "<path>", "string", "[background].image", "Path to background bitmap. Relative CLI paths resolve against the current working directory.", "if omitted, defers to config.toml [background].image; if missing there, uses [\"\"] (disabled)."),
        new("--background-fit", "<mode>", "string", "[background].fit", "Background fit mode.", "if omitted, defers to config.toml [background].fit; if missing there, uses [\"CropToFill\"]."),
        new("--background-alternating", "<bool>", "bool", "[background].alternating", "Enable alternating-pixel pattern.", "if omitted, defers to config.toml [background].alternating; if missing there, uses [false]."),
        new("--background-border", "<bool>", "bool", "[background].border", "Enable viewport border.", "if omitted, defers to config.toml [background].border; if missing there, uses [false]."),
        new("--background-border-color", "<color>", "string", "[background].border-color", "Border color.", "if omitted, defers to config.toml [background].border-color; if missing there, uses [\"#FFFFFF\"]."),
        new("--grid-size", "<dim>", "string", "[grid].size", "Grid cell side length.", "if omitted, defers to config.toml [grid].size; if missing there, uses [\"100px\"]."),
        new("--grid-odd-color", "<color>", "string", "[grid].odd-color", "Odd-cell color.", "if omitted, defers to config.toml [grid].odd-color; if missing there, uses [\"#00000080\"]."),
        new("--grid-even-color", "<color>", "string", "[grid].even-color", "Even-cell color.", "if omitted, defers to config.toml [grid].even-color; if missing there, uses [\"transparent\"]."),
        new("--grid-stroke", "<dim>", "string", "[grid].stroke", "Cell stroke width.", "if omitted, defers to config.toml [grid].stroke; if missing there, uses [\"0\"]."),
        new("--grid-offset-x", "<dim>", "string", "[grid].offset-x", "Grid origin X.", "if omitted, defers to config.toml [grid].offset-x; if missing there, uses [\"0\"]."),
        new("--grid-offset-y", "<dim>", "string", "[grid].offset-y", "Grid origin Y.", "if omitted, defers to config.toml [grid].offset-y; if missing there, uses [\"0\"]."),
        new("--grid-coordinates", "<bool>", "bool", "[grid].coordinates", "Enable per-cell coordinate labels.", "if omitted, defers to config.toml [grid].coordinates; if missing there, uses [false]."),
        new("--circle-size", "<dim>", "string", "[circle].size", "Circle diameter.", "if omitted, defers to config.toml [circle].size; if missing there, uses [\"100vmin\"]."),
        new("--circle-x", "<dim>", "string", "[circle].x", "Circle center X.", "if omitted, defers to config.toml [circle].x; if missing there, uses [\"50vw\"]."),
        new("--circle-y", "<dim>", "string", "[circle].y", "Circle center Y.", "if omitted, defers to config.toml [circle].y; if missing there, uses [\"50vh\"]."),
        new("--circle-color", "<color>", "string", "[circle].color", "Circle color.", "if omitted, defers to config.toml [circle].color; if missing there, uses [\"#ffffff40\"]."),
        new("--circle-stroke", "<dim>", "string", "[circle].stroke", "Circle stroke width.", "if omitted, defers to config.toml [circle].stroke; if missing there, uses [\"0\"]."),
        new("--crosshair-length", "<dim>", "string", "[crosshair].length", "Crosshair half-arm length.", "if omitted, defers to config.toml [crosshair].length; if missing there, uses [\"5vmin\"]."),
        new("--crosshair-x", "<dim>", "string", "[crosshair].x", "Crosshair center X.", "if omitted, defers to config.toml [crosshair].x; if missing there, uses [\"50vw\"]."),
        new("--crosshair-y", "<dim>", "string", "[crosshair].y", "Crosshair center Y.", "if omitted, defers to config.toml [crosshair].y; if missing there, uses [\"50vh\"]."),
        new("--crosshair-color", "<color>", "string", "[crosshair].color", "Crosshair color.", "if omitted, defers to config.toml [crosshair].color; if missing there, uses [\"#ffffff80\"]."),
        new("--crosshair-stroke", "<dim>", "string", "[crosshair].stroke", "Crosshair stroke width.", "if omitted, defers to config.toml [crosshair].stroke; if missing there, uses [\"1px\"]."),
        new("--logo-source", "<path>", "string", "[logo].source", "Path to logo file (PNG/JPG/SVG) or pack URI. Empty string suppresses logo. Relative CLI file paths resolve against the current working directory.", "if omitted, defers to config.toml [logo].source; if missing there, uses the embedded logo via pack URI."),
        new("--logo-x", "<dim>", "string", "[logo].x", "Logo center X.", "if omitted, defers to config.toml [logo].x; if missing there, uses [\"85vw\"]."),
        new("--logo-y", "<dim>", "string", "[logo].y", "Logo center Y.", "if omitted, defers to config.toml [logo].y; if missing there, uses [\"15vh\"]."),
        new("--logo-width", "<dim>", "string", "[logo].width", "Logo rect width.", "if omitted, defers to config.toml [logo].width; if missing there, uses [\"20vw\"]."),
        new("--logo-height", "<dim>", "string", "[logo].height", "Logo rect height.", "if omitted, defers to config.toml [logo].height; if missing there, uses [\"20vh\"]."),
        new("--logo-opacity", "<f|[f1,f2]>", "float|float[]", "[logo].opacity", "Logo alpha multiplier(s) in range [0, 1]; accepts a single float or a TOML float array literal.", "if omitted, defers to config.toml [logo].opacity; if missing there, uses [1.0]."),
        new("--no-assignment", "<bool>", "bool", "[render].no-assignment", "Generate PNGs without assigning wallpaper.", "if omitted, defers to config.toml [render].no-assignment; if missing there, uses false."),
        new("--no-discovery", "<bool>", "bool", "[render].no-discovery", "Skip display discovery and render only configured [[output]] entries using each [output.hardware_output]. Implies --no-assignment.", "if omitted, defers to config.toml [render].no-discovery; if missing there, uses false."),
        new("--outputs-skip-unspecified", "<bool>", "bool", "[render].outputs-skip-unspecified", "Skip discovered displays that have no explicit [[output]] target.", "if omitted, defers to config.toml [render].outputs-skip-unspecified; if missing there, uses false."),
        new("--render-output", "<path>", "string", "[render].output", "Output path template (directory + filename stem) for generated PNGs. Supports {now}, {index}, {friendlyName} tokens. Relative CLI paths resolve against the current working directory.", "if omitted, defers to config.toml [render].output; if missing there, defaults to %TEMP%/BgRaster/{now}_{index}."),
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