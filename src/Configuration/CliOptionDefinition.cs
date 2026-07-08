// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Configuration;

internal sealed record CliOptionDefinition(
    string Alias,
    string? ValueSyntax,
    string TypeName,
    string TomlEquivalent,
    string Description,
    string DefaultResolution,
    string Category)
{
    internal string OptionSyntax => ValueSyntax is null ? Alias : $"{Alias} {ValueSyntax}";

    internal string HelpDescription => $"{Description} Default resolution: {DefaultResolution}";
}