namespace GameshowPro.BgRaster.Models;

record GridOptions
{
    internal ImmutableArray<string> Size { get; init; } = ["100px"];
    internal ImmutableArray<string> OddColor { get; init; } = ["#00000080"];
    internal ImmutableArray<string> EvenColor { get; init; } = ["transparent"];
    internal ImmutableArray<string> Stroke { get; init; } = ["0"];
    internal ImmutableArray<string> OffsetX { get; init; } = ["0"];
    internal ImmutableArray<string> OffsetY { get; init; } = ["0"];
    internal ImmutableArray<bool> Coordinates { get; init; } = [false];
}
