namespace GameshowPro.BgRaster.Models;

record LabeledEdgesOverride
{
    internal string? TextSize { get; init; }
    internal string? TailLength { get; init; }
    internal string? Thickness { get; init; }
    internal float? HeadScale { get; init; }
    internal string? Scope { get; init; }
    internal ImmutableArray<LabeledEdgeSide>? Side { get; init; }
}