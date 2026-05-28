namespace GameshowPro.BgRaster.Models;

record LogoOverride
{
    internal string? Source { get; init; }
    internal string? X { get; init; }
    internal string? Y { get; init; }
    internal string? Width { get; init; }
    internal string? Height { get; init; }
    internal float? Opacity { get; init; }
}
