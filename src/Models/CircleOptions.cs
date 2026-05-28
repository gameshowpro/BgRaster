namespace GameshowPro.BgRaster.Models;

record CircleOptions
{
    internal ImmutableArray<string> Size { get; init; } = ["100vmin"];
    internal ImmutableArray<string> Color { get; init; } = ["#ffffff40"];
    internal ImmutableArray<string> Stroke { get; init; } = ["0"];
}
