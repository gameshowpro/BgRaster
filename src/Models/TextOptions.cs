namespace GameshowPro.BgRaster.Models;

record TextOptions
{
    internal ImmutableArray<string> Text { get; init; } = ["${MachineName} ${Index}", "${OutputName}", "${Width}x${Height}"];
    internal ImmutableArray<string> Size { get; init; } = ["3vh", "2vh", "4vh"];
    internal ImmutableArray<string> Color { get; init; } = ["#fff"];
    internal ImmutableArray<string> X { get; init; } = ["85vw"];
    internal ImmutableArray<string> Y { get; init; } = ["85vh"];
}
