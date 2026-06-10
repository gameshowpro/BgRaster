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
