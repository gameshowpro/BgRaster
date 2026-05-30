namespace GameshowPro.BgRaster.Models;

record OutputOptions
{
    internal OutputTarget Target { get; init; } = OutputTarget.FromIndex(0);
    internal TextOverride? Text { get; init; }
    internal BackgroundOverride? Background { get; init; }
    internal GridOverride? Grid { get; init; }
    internal CircleOverride? Circle { get; init; }
    internal CrosshairOverride? Crosshair { get; init; }
    internal LabeledEdgesOverride? LabeledEdges { get; init; }
    internal LogoOverride? Logo { get; init; }
    internal ImmutableArray<SliceOptions> Slices { get; init; } = [];
}
