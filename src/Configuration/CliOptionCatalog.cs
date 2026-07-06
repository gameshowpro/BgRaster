// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Configuration;

internal static class CliOptionCatalog
{
    internal const string OptionsTableBeginMarker = "<!-- BEGIN:CLI_OPTIONS_TABLE -->";
    internal const string OptionsTableEndMarker = "<!-- END:CLI_OPTIONS_TABLE -->";

    internal static ImmutableArray<CliOptionDefinition> Definitions { get; } = GeneratedCliOptionCatalog.Definitions;

    internal static CliOptionDefinition GetByAlias(string alias) =>
        Definitions.First(d => string.Equals(d.Alias, alias, StringComparison.Ordinal));

    internal static string BuildOptionsMarkdownTable()
    {
        StringBuilder sb = new();
        _ = sb.AppendLine("| Option | Type | TOML equivalent | Description | Default resolution |");
        _ = sb.AppendLine("|---|---|---|---|---|");

        foreach (CliOptionDefinition definition in Definitions)
        {
            _ = sb.Append("| `");
            _ = sb.Append(definition.OptionSyntax);
            _ = sb.Append("` | `");
            _ = sb.Append(definition.TypeName);
            _ = sb.Append("` | `");
            _ = sb.Append(definition.TomlEquivalent);
            _ = sb.Append("` | ");
            _ = sb.Append(definition.Description);
            _ = sb.Append(" | ");
            _ = sb.Append(definition.DefaultResolution);
            _ = sb.AppendLine(" |");
        }

        return sb.ToString().TrimEnd();
    }
}