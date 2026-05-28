namespace GameshowPro.BgRaster.Models;

record SliceOptions
{
    internal string X { get; init; } = "0";
    internal string Y { get; init; } = "0";
    internal string Width { get; init; } = "100vw";
    internal string Height { get; init; } = "100vh";
    internal TextOverride? Text { get; init; }
    internal BackgroundOverride? Background { get; init; }
    internal GridOverride? Grid { get; init; }
    internal CircleOverride? Circle { get; init; }
    internal CrosshairOverride? Crosshair { get; init; }
    internal LogoOverride? Logo { get; init; }
}
