namespace GameshowPro.BgRaster.Models;

record CircleOptions
{
    internal ImmutableArray<string> X { get; init; } = ["50vw"];
    internal ImmutableArray<string> Y { get; init; } = ["50vh"];
    internal ImmutableArray<string> Size { get; init; } = ["100vmin"];
    internal ImmutableArray<string> Color { get; init; } = ["#ffffff40"];
    internal ImmutableArray<string> Stroke { get; init; } = ["0"];
}
