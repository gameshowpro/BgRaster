namespace GameshowPro.BgRaster.Configuration;

static class CliOptionCatalog
{
    internal const string OptionsTableBeginMarker = "<!-- BEGIN:CLI_OPTIONS_TABLE -->";
    internal const string OptionsTableEndMarker = "<!-- END:CLI_OPTIONS_TABLE -->";

    internal static ImmutableArray<CliOptionDefinition> Definitions { get; } = GeneratedCliOptionCatalog.Definitions;

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