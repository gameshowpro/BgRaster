namespace GameshowPro.BgRaster.Models;

record LogoOptions
{
    internal ImmutableArray<string> Source { get; init; } = [""];
    internal ImmutableArray<string> X { get; init; } = ["85vw"];
    internal ImmutableArray<string> Y { get; init; } = ["15vh"];
    internal ImmutableArray<string> Width { get; init; } = ["10vw"];
    internal ImmutableArray<string> Height { get; init; } = ["10vh"];
    internal ImmutableArray<float> Opacity { get; init; } = [1f];
}
