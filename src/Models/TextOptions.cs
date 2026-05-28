namespace GameshowPro.BgRaster.Models;

record TextOptions
{
    internal ImmutableArray<string> Title { get; init; } = ["${MachineName} ${Index}"];
    internal ImmutableArray<string> Subtitle { get; init; } = ["${Width}x${Height}"];
    internal ImmutableArray<string> Size { get; init; } = ["1vh"];
    internal ImmutableArray<string> X { get; init; } = ["75vw"];
    internal ImmutableArray<string> Y { get; init; } = ["75vh"];
}
