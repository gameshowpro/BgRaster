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

    internal static ImmutableArray<CliOptionDefinition> GetByCategory(string category) =>
        Definitions.Where(d => string.Equals(d.Category, category, StringComparison.Ordinal)).ToImmutableArray();

    internal static string BuildOptionsMarkdownTable()
    {
        string[] categories = ["Frequent", "Advanced", "Appearance"];

        StringBuilder sb = new();

        foreach (string category in categories)
        {
            ImmutableArray<CliOptionDefinition> group = GetByCategory(category);
            if (group.IsDefaultOrEmpty)
                continue;

            _ = sb.Append("### ").Append(category).AppendLine(" options");
            _ = sb.AppendLine();
            _ = sb.AppendLine("| Option | Type | TOML equivalent | Description | Default resolution |");
            _ = sb.AppendLine("|---|---|---|---|---|");

            foreach (CliOptionDefinition definition in group)
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

            _ = sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
}