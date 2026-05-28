namespace GameshowPro.BgRaster.Models;

record BackgroundOverride
{
    internal string? Color { get; init; }
    internal string? Image { get; init; }
    internal string? Fit { get; init; }
    internal bool? Alternating { get; init; }
    internal bool? Border { get; init; }
    internal string? BorderColor { get; init; }
}
